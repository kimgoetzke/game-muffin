using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHindsight.Core;
using CaptainHindsight.Directors;
using CaptainHindsight.Managers;
using CaptainHindsight.Player;
using Sirenix.OdinInspector;
using UnityEngine;
using static CaptainHindsight.Quests.Quest;
using static CaptainHindsight.Quests.QuestData;

namespace CaptainHindsight.Quests
{
  public class QuestManager : MonoBehaviour, IPlayerPrefsSaveable
  {
    public static QuestManager Instance;

    #region Defining variables

    [TitleGroup("Available quests")]
    [InfoBox(
      "If you update any tasks in the Unity Inspector, you need to update QuestStates manually by pressing 'Update All Quest States' above. Note that QuestState.Completed quests will not be updated when pressing the button.")]
    [SerializeField]
    private List<Quest> quests = new();

    [TitleGroup("Available quests")] private DialogueVariables _dialogueVariables;

    #endregion

    #region Configuring Unity Inspector-only buttons

    [TitleGroup("Editor actions")]
    [Button("Bootstrap all tasks and variables", ButtonSizes.Large)]
    private void BootstrapAllDataViaEditor()
    {
      if (Application.isPlaying)
      {
        if (quests is { Count: > 0 } && quests[0].tasks is { Count: > 0 })
        {
          Helper.LogWarning(
            "[QuestManager] Quest list already contains data. Remove all quest task data and quest variable data it before bootstrapping.");
          return;
        }

        quests?.ForEach(q =>
        {
          if (q.questData == null) return;
          q.Reset(quests);
          q.questData.tasks.ForEach(t =>
          {
            var newTask = new QuestTask();
            q.tasks.Add(newTask);
            newTask.questTaskData = t;
          });

          q.questData.questVariables.ForEach(v =>
          {
            var newVariable = new QuestVariable();
            q.questVariables.Add(newVariable);
            newVariable.questVariableData = v;
          });
        });
      }
      else
      {
        Helper.LogWarning("[QuestManager] This function only works in Play mode.");
      }
    }

    [HorizontalGroup("Editor actions/Split")]
    [PropertyOrder(1)]
    [VerticalGroup("Editor actions/Split/Right")]
    [Button("Reset All Quests & Tasks", ButtonSizes.Large)]
    [GUIColor(1, 0.2f, 0)]
    private void ResetAllQuests()
    {
      quests.ForEach(q => q.Reset(quests));
    }

    [VerticalGroup("Editor actions/Split/Left")]
    [Button("Update All Quest States", ButtonSizes.Large)]
    [GUIColor(0, 1, 0)]
    private void UpdateAllQuestStatesViaTheEditor()
    {
      if (Application.isPlaying == false)
        RefreshAllIncompleteQuestStates();
      else if (quests == null || quests.Count == 0)
        Helper.LogWarning("[QuestManager] This function only works in Play mode.");
    }

    [Button("Print All Ink Dialogue Variables & Their Values", ButtonSizes.Large)]
    [PropertyOrder(0)]
    [PropertySpace(SpaceBefore = 10)]
    private void PrintVariableValues()
    {
      if (Application.isPlaying == false)
      {
        Helper.LogWarning("[QuestManager] This function only works in Play mode.");
        return;
      }
      else if (_dialogueVariables == null)
      {
        Helper.LogWarning(
          "[QuestManager] There are no dialogue variables. This is unexpected. Please investigate why this has happened.");
        return;
      }

      Helper.Log("[QuestManager] List of current global Ink variables (public version):");
      foreach (var kvp in _dialogueVariables.Variables)
        Helper.Log(" - " + kvp.Key + " = " + kvp.Value);
    }

    #endregion

    #region Awake, Start & initialisation

    private void Awake()
    {
      if (Instance == null)
      {
        Instance = this;
      }
      else
      {
        Destroy(gameObject);
        return;
      }

      RefreshAllIncompleteQuestStates();
    }

    private void Start() => TryLoadPlayerPrefs();

    public Quest GetQuest(QuestData questData)
    {
      return quests.Find(q => q.questData.questName == questData.questName);
    }

    #endregion

    #region Updating tasks (public, used by MQuestDefeatable/MQuestLocation/etc.)

    public void UpdateTask(TaskType type, Transform reachedObject = null,
      NpcIdentifier identifier = NpcIdentifier.Unspecified)
    {
      foreach (var quest in quests)
      {
        // Only go through QuestState.Active
        if (quest.state != QuestState.Active) continue;

        // Loop through all tasks within the quest and update them, if applicable
        foreach (var task in quest.tasks)
          switch (type)
          {
            case TaskType.Do:
              break;
            case TaskType.Defeat:
              if (task.questTaskData.npcIdentifier != identifier) continue;
              task.defeated++;
              InvokeChangeQuestStateEvent(quest);
              // Helper.Log("[QuestManager] Defeat quest: " + identifier + ", count = " + quests[i].Tasks[j].Defeated + ".");
              break;
            case TaskType.Collect:
              break;
            case TaskType.Reach:
              Helper.Log("[QuestManager] Reach quest: Evaluating location.");
              var locationReached = task.AtLocation(reachedObject);
              if (locationReached)
                InvokeChangeQuestStateEvent(quest);
              break;
            default:
              Helper.LogWarning("[QuestManager] Task type (" + type + ") to be updated does not exist.");
              break;
          }

        // Loop through all tasks within the same quest again and check whether
        // all tasks have been completed
        var thereAreOutstandingTasksLeft = false;
        foreach (var task in quest.tasks)
          if (task.State == false)
            thereAreOutstandingTasksLeft = true;

        // If yes, set QuestState.Ready
        if (thereAreOutstandingTasksLeft == false)
        {
          SetQuestStateToReady(quest);
          SetQuestAndDialogueVariableToReady(quest);
        }
      }
    }

    #endregion

    #region Managing QuestState

    private void RefreshAllIncompleteQuestStates()
    {
      foreach (var quest in quests)
      {
        Helper.Log("[QuestManager] Refreshing quest state for " + quest.questData.questName +
                   "...");
        quest.SetRequirementStatus(quests);

        if (quest.state == QuestState.Completed) continue;

        // Loop through all tasks in a quest to find out if 1) any and/or 2) all
        // tasks have been completed
        var someTasksAreOutstanding = false;
        var someTasksHaveBeenCompleted = false;
        foreach (var task in quest.tasks)
        {
          if (task.State == false) someTasksAreOutstanding = true;
          else someTasksHaveBeenCompleted = true;
        }

        switch (someTasksAreOutstanding) // Set QuestState if...
        {
          case true when someTasksHaveBeenCompleted == false: // ...no tasks have been completed yet
          {
            quest.state = quest.SetRequirementStatus(quests)
              ? QuestState.Available // ...but requirements fulfilled
              : QuestState.Unavailable; // ...and requirements not fulfilled
            break;
          }
          case true: // ...some task have been completed
            SetQuestStateToActive(quest);
            break;
          case false: // ...all tasks have been completed
          {
            if (quest.state is not QuestState.Ready and not QuestState.Completed)
            {
              SetQuestStateToReady(quest);
              SetQuestAndDialogueVariableToReady(quest);
            }

            break;
          }
        }
      }
    }

    private static void SetQuestStateToActive(Quest quest)
    {
      AudioDirector.Instance.Play("Progression");
      quest.state = QuestState.Active;
    }

    private static void SetQuestStateToReady(Quest quest)
    {
      AudioDirector.Instance.Play("Progression");
      quest.state = QuestState.Ready;
    }

    private static void SetQuestStateToCompleted(Quest quest)
    {
      AudioDirector.Instance.Play("Progression");
      quest.state = QuestState.Completed;
    }

    private async void InvokeChangeQuestStateEvent(Quest quest, float delay,
      bool onlyStateChange = false)
    {
      // The option to delay the event is necessary for events that trigger right after start-up 
      // as the QuestManager is initialised before most components (see Script Execution Order)
      // and events could therefore be missed by other components, esp. MQuestTracker.
      await Task.Delay(TimeSpan.FromSeconds(delay));
      InvokeChangeQuestStateEvent(quest, onlyStateChange);
    }

    private static void InvokeChangeQuestStateEvent(Quest quest, bool onlyStateChange = false)
    {
      EventManager.Instance.ChangeQuestState(quest, onlyStateChange);
    }

    #endregion

    #region Managing QuestVariables & DialogueVariables

    private void SetDialogueVariables(DialogueVariables variables)
    {
      _dialogueVariables = variables;
    }

    private async void SetQuestAndDialogueVariableToReady(Quest quest)
    {
      if (quest.questData.story == null) return;

      await Task.Delay(TimeSpan.FromSeconds(0.25f));

      foreach (var variable in quest.questVariables)
      {
        if (variable.questVariableData.category != VariableCategory.QuestReady) continue;
        variable.boolValue = true;
        Ink.Runtime.Object obj = new Ink.Runtime.BoolValue(true);
        _dialogueVariables.UpdateDictionary(variable.questVariableData.name, obj);
      }
    }

    // NOTE: The below are not implemented yet as they were not needed but they will
    // be critical for any non-completion state QuestVariable
    private void SetQuestAndDialogueVariable(Quest quest, VariableCategory category, string value)
    {
      if (category is VariableCategory.QuestAccepted or VariableCategory.QuestComplete)
      {
        Helper.LogWarning(
          "[QuestManager] Invalid request. You're trying to set a variable that is controlled by Ink. Request ignored.");
        return;
      }

      foreach (var variable in quest.questVariables)
      {
        if (variable.questVariableData.category != category) continue;
        variable.stringValue = value;
        Ink.Runtime.Object obj = new Ink.Runtime.StringValue(value);
        _dialogueVariables.UpdateDictionary(variable.questVariableData.name, obj);
      }
    }

    private void SetQuestAndDialogueVariable(Quest quest, VariableCategory category, bool value)
    {
      if (category is VariableCategory.QuestAccepted or VariableCategory.QuestComplete)
      {
        Helper.LogWarning(
          "[QuestManager] Invalid request. You're trying to set a variable that is controlled by Ink. Request ignored.");
        return;
      }

      foreach (var variable in quest.questVariables)
      {
        if (variable.questVariableData.category != category) continue;
        variable.boolValue = value;
        Ink.Runtime.Object obj = new Ink.Runtime.BoolValue(value);
        _dialogueVariables.UpdateDictionary(variable.questVariableData.name, obj);
      }
    }

    private void SetQuestAndDialogueVariable(Quest quest, VariableCategory category, int value)
    {
      if (category is VariableCategory.QuestAccepted or VariableCategory.QuestComplete)
      {
        Helper.LogWarning(
          "[QuestManager] Invalid request. You're trying to set a variable that is controlled by Ink. Request ignored.");
        return;
      }

      foreach (var variable in quest.questVariables)
      {
        if (variable.questVariableData.category != category) continue;
        variable.intValue = value;
        Ink.Runtime.Object obj = new Ink.Runtime.IntValue(value);
        _dialogueVariables.UpdateDictionary(variable.questVariableData.name, obj);
      }
    }

    // This function is triggered by MButtonInGame through an event when the
    // player leaves the interactionRadius of the MButtonInGame - its purpose
    // is to make sure that the game is aware of any updates to variables in
    // Ink which are relevant to the game
    private void UpdateQuestVariables(string storyName)
    {
      if (_dialogueVariables == null)
      {
        Helper.LogWarning("[QuestManager] dialogueVariables = null, mate. WTF?");
        return;
      }

      Helper.Log(
        "[QuestManager] Quest variables are being updated because player left interactionRadius of MButtonInGame.");
      foreach (var quest in quests)
      {
        if (quest.questData.name != storyName) continue;
        UpdateQuestVariables(quest);
        return;
      }

      Helper.LogWarning("[QuestManager] Couldn't update quest variables for '" + storyName +
                        "'. Quest not found.");
    }

    private void UpdateQuestVariables(Quest quest)
    {
      Helper.Log("[QuestManager] Updating: '" + quest.questData.name + "'.");
      foreach (var variable in quest.questVariables)
      {
        switch (variable.questVariableData.type)
        {
          case VariableType.String:
            variable.stringValue = _dialogueVariables.ReturnString(variable.questVariableData.name);
            Helper.Log(" - " + variable.questVariableData.name + "=" + variable.stringValue);
            break;
          case VariableType.Bool:
            variable.boolValue = _dialogueVariables.ReturnBool(variable.questVariableData.name);
            Helper.Log(" - " + variable.questVariableData.name + "=" + variable.boolValue);
            break;
          case VariableType.Int:
            variable.intValue = _dialogueVariables.ReturnInt(variable.questVariableData.name);
            Helper.Log(" - " + variable.questVariableData.name + "=" + variable.intValue);
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }

      UpdateQuestStateAfterVariableChange(quest);
    }

    private void UpdateQuestStateAfterVariableChange(Quest quest)
    {
      if (quest.state is QuestState.Active or QuestState.Completed)
      {
        Helper.Log("[QuestManager] Request to update quest state ignored because '" +
                   quest.questData.questName + "' is '" + quest.state +
                   "' which is a state that is only modified through dialogue i.e. by Ink.");
        return;
      }

      // Loop through all QuestVariables to look for VariableCategory.QuestComplete
      // and VariableCategory.QuestAccepted, and update QuestState - break loop if
      // the former is true
      foreach (var variable in quest.questVariables)
      {
        Helper.Log($"[QuestManager] Evaluating: '{variable.questVariableData.name}' ({variable.boolValue}).");
        switch (variable.questVariableData.category)
        {
          case VariableCategory.QuestComplete
            when variable.boolValue
                 && quest.state == QuestState.Ready:
            SetQuestStateToCompleted(quest);
            InvokeChangeQuestStateEvent(quest, true);
            break;
          case VariableCategory.QuestAccepted
            when variable.boolValue
                 && quest.state != QuestState.Ready
                 && quest.state != QuestState.Completed:
            SetQuestStateToActive(quest);
            InvokeChangeQuestStateEvent(quest, true);
            continue;
          case VariableCategory.Unspecified:
          case VariableCategory.QuestReady:
          case VariableCategory.KnowsNpc:
          default:
            continue;
        }
      }
    }

    #endregion

    #region Managing events

    private void OnEnable()
    {
      EventManager.Instance.OnDialogueVariablesShare += SetDialogueVariables;
      EventManager.Instance.OnInteractionTriggerRadiusExit += UpdateQuestVariables;
      EventManager.Instance.OnSceneExit += TrySavePlayerPrefs;
    }

    private void OnDestroy()
    {
      EventManager.Instance.OnDialogueVariablesShare -= SetDialogueVariables;
      EventManager.Instance.OnInteractionTriggerRadiusExit -= UpdateQuestVariables;
      EventManager.Instance.OnSceneExit -= TrySavePlayerPrefs;
    }

    #endregion

    #region Managing saving and loading

    public void TryLoadPlayerPrefs()
    {
      var questsToLoad = PlayerPrefsManager.Instance.LoadQuests();
      if (questsToLoad.Count == 0) return;
      foreach (var qtl in questsToLoad)
      {
        var quest = quests.Find(q => q.name == qtl.name);
        if (quests == null)
        {
          Helper.LogWarning($"[QuestManager] Quest '{quest.name}' not found and not loaded.");
          continue;
        }

        Helper.Log($"[QuestManager] Quest '{qtl.name}' loaded from PlayerPrefs.");
        Helper.Log($"[QuestManager] Quest '{quest.name}' found in list and loaded.");
        quest.state = qtl.state;
        quest.tasks = qtl.tasks;
        UpdateQuestVariables(quest);
        if (quest.state is QuestState.Active or QuestState.Ready)
        {
          InvokeChangeQuestStateEvent(quest, 0.1f, true);
        }
      }
    }

    public void TrySavePlayerPrefs(string spawnPointName)
    {
      // var questsWithoutData = new List<Quest>(); 
      // foreach (var quest in quests)
      // {
      //   var q = new Quest
      //   {
      //     name = quest.name,
      //     questData = null,
      //     state = quest.state,
      //     questRequirementStatus = quest.questRequirementStatus,
      //     questVariables = quest.questVariables,
      //     tasks = quest.tasks,
      //   };
      //   questsWithoutData.Add(q);
      // }
      // PlayerPrefsManager.Instance.Save(questsWithoutData);
      PlayerPrefsManager.Instance.Save(quests);
    }

    #endregion
  }
}