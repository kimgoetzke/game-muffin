using System;
using System.Collections.Generic;
using CaptainHindsight.Core;
using CaptainHindsight.Managers;
using CaptainHindsight.Player;
using CaptainHindsight.UI;
using Ink.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CaptainHindsight.Quests
{
  public class DialogueManager : MonoBehaviour, IPlayerPrefsSaveable
  {
    public static DialogueManager Instance;

    [FormerlySerializedAs("globalVariablesJSON")]
    [Title("Configuration")]
    [SerializeField]
    [Required]
    private TextAsset globalVariablesJson;

    [ShowInInspector] [ReadOnly] private List<Stories> _listOfStories = new();
    private Stories _currentActiveStory;
    private DialogueCanvasController _dialogueCanvasController;
    public Story CurrentStory { get; private set; }
    public bool inProgress;
    private bool _canContinue = true;
    private DialogueVariables _dialogueVariables;

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

      // Find DialogueUI, the canvas for all dialogue-related UI
      if (GameObject.Find("UI/DialogueUI").GetComponent<DialogueCanvasController>() == null)
        Debug.LogError("[QuestGiver] Dialogue UI canvas not found.");
      _dialogueCanvasController =
        GameObject.Find("UI/DialogueUI").GetComponent<DialogueCanvasController>();

      // Get instance of dialogue variables class
      TryLoadPlayerPrefs();
      EventManager.Instance.ShareDialogueVariables(_dialogueVariables);
    }

    #region Managing dialogue progression

    private void TriggerDialogue(TextAsset inkJson, string speaker)
    {
      EventManager.Instance.ChangeDialogueState(true);
      if (inProgress == false)
      {
        inProgress = true;
        StartDialogue(inkJson, speaker);
      }
      else
      {
        ContinueDialogue();
      }
    }

    private void DialogueContinuationViaUIButton()
    {
      ContinueDialogue();
    }

    private void StartDialogue(TextAsset inkJson, string speaker)
    {
      // Set currentStory based on input
      CurrentStory = new Story(inkJson.text);
      if (OnCreateStory != null) OnCreateStory(CurrentStory);

      // Add story to list of active stories
      var thisNewStory = new Stories
        { Speaker = speaker, Story = CurrentStory, InkJson = inkJson };
      _currentActiveStory = thisNewStory;
      _listOfStories.Add(thisNewStory);

      // Update globalVariables in currentStory and subscribe to changes going forward
      _dialogueVariables.StartListening(CurrentStory);

      // Set DialogueCanvas active and set speaker
      _dialogueCanvasController.SetCanvasActive(true);
      _dialogueCanvasController.icon.enabled = true;
      _dialogueCanvasController.speakerMesh.text = speaker;

      ContinueDialogue();
    }

    private void ContinueDialogue()
    {
      if (_dialogueCanvasController.GetCanvasStatus() == false)
      {
        _dialogueCanvasController.SetCanvasActive(true);
        return;
      }

      if (CurrentStory.canContinue)
      {
        _dialogueCanvasController.textMesh.text = CurrentStory.Continue();
        DisplayChoicesOrIcon();
      }
      else
      {
        EndDialogue();
      }
    }

    private void DisplayChoicesOrIcon()
    {
      var currentChoices = CurrentStory.currentChoices;

      if (currentChoices.Count == 0)
      {
        _dialogueCanvasController.icon.enabled = true;
        _dialogueCanvasController.SetChoicesGroupActive(false);
      }
      else
      {
        _dialogueCanvasController.icon.enabled = false;
        _dialogueCanvasController.SetChoicesGroupActive(true);
        _dialogueCanvasController.DisplayChoices(currentChoices);
      }
    }

    private void MakeChoice(int choiceIndex)
    {
      if (_canContinue)
      {
        CurrentStory.ChooseChoiceIndex(choiceIndex);
        _dialogueCanvasController.SetChoicesGroupActive(false);
        ContinueDialogue();
      }
    }

    private void PauseDialogue(string storyName)
    {
      if (inProgress == false) return;
      Helper.Log("[DialogueManager] Dialogue paused. Canvas deactivated.");
      EventManager.Instance.ChangeDialogueState(false);
      _dialogueCanvasController.SetCanvasActive(false);
      inProgress = false;
    }

    private void EndDialogue()
    {
      inProgress = false;
      EventManager.Instance.ChangeDialogueState(false);
      _dialogueVariables.StopListening(CurrentStory);
      _dialogueCanvasController.SetCanvasActive(false);
      _dialogueCanvasController.icon.enabled = true;
      _dialogueCanvasController.speakerMesh.text = "Name";
      _dialogueCanvasController.textMesh.text = "...";
      _currentActiveStory.CanContinue = false;
      CurrentStory = null;
    }

    #endregion

    #region Managing global story variables

    // No longer used, should possibly be deleted
    public Ink.Runtime.Object GetVariable(string variableName)
    {
      Ink.Runtime.Object variableValue = null;
      _dialogueVariables.Variables.TryGetValue(variableName, out variableValue);
      if (variableValue == null)
        Debug.LogWarning("[DialogueManager] Ink Variable was found to be null: " + variableName);

      return variableValue;
    }

    #endregion

    #region Managing events

    public static event Action<Story> OnCreateStory;

    private void OnEnable()
    {
      EventManager.Instance.OnDialogueTrigger += TriggerDialogue;
      EventManager.Instance.OnDialogueContinueTrigger += DialogueContinuationViaUIButton;
      EventManager.Instance.OnInteractionTriggerRadiusExit += PauseDialogue;
      EventManager.Instance.OnDialogueChoiceSubmission += MakeChoice;
      EventManager.Instance.OnSceneExit += TrySavePlayerPrefs;
    }

    private void OnDestroy()
    {
      EventManager.Instance.OnDialogueTrigger -= TriggerDialogue;
      EventManager.Instance.OnDialogueContinueTrigger -= DialogueContinuationViaUIButton;
      EventManager.Instance.OnInteractionTriggerRadiusExit -= PauseDialogue;
      EventManager.Instance.OnDialogueChoiceSubmission -= MakeChoice;
      EventManager.Instance.OnSceneExit -= TrySavePlayerPrefs;
    }

    #endregion

    #region Managing saving and loading

    public void TryLoadPlayerPrefs()
    {
      var jsonState = PlayerPrefsManager.Instance.LoadString(GlobalConstants.DIALOGUE_VARIABLES);
      if (jsonState.Length > 0)
      {
        _dialogueVariables = new DialogueVariables(globalVariablesJson, jsonState);
        return;
      }

      _dialogueVariables = new DialogueVariables(globalVariablesJson);
    }

    public void TrySavePlayerPrefs(string spawnPointName)
    {
      PlayerPrefsManager.Instance.Save(GlobalConstants.DIALOGUE_VARIABLES,
        _dialogueVariables.ToJson());
    }

    #endregion

    #region Managing active stories master list

    public class Stories
    {
      public string Speaker;
      public TextAsset InkJson;
      public Story Story;
      public bool CanContinue;
    }

    #endregion
  }
}