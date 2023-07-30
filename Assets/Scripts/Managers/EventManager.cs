using System;
using CaptainHindsight.Core;
using CaptainHindsight.Quests;
using UnityEngine;

namespace CaptainHindsight.Managers
{
  public class EventManager : MonoBehaviour
  {
    public static EventManager Instance;

    private void Awake()
    {
      if (Instance == null)
      {
        Instance = this;
      }
      else
      {
        Destroy(gameObject);
      }
    }

    #region Managing PlayerPrefs related events

    public event Action<string> OnSceneExit;

    // Used by ZCutSceneTrigger when player enters a cut scene zone in order to notify
    // all classes that implement IPlayerPrefsSaveable to contact PlayerPrefsManager
    public void ExitScene(string spawnPointName)
    {
      OnSceneExit?.Invoke(spawnPointName);
    }

    #endregion

    #region Managing experience and skill-related events

    public event Action<int, int> OnExperienceChange;
    public event Action<int> OnSkillPointChange;
    public event Action OnActiveSkillsChange;

    // Used by ExperienceManager when experience is gained which will update the UI via MExp
    public void ChangeExperience(int experience, int maxExperience)
    {
      OnExperienceChange?.Invoke(experience, maxExperience);
    }

    // Used by ExperienceManager when a new skill point is gained and by PlayerSkillsManager
    // when a skill point is spent
    public void ChangeSkillPoints(int change)
    {
      OnSkillPointChange?.Invoke(change);
    }

    // Used by PlayerSkillsManager to inform all classes relevant about new
    // unlocked skills e.g. PlayerManager, EquipmentController, etc.
    public void UpdateActiveSkills()
    {
      OnActiveSkillsChange?.Invoke();
    }

    #endregion

    #region Managing player & player input-related events
    
    public event Action OnPauseMenuRequest;

    // Used by PlayerController when relevant input is detected (e.g. Escape key pressed)
    public void RequestPauseMenu()
    {
      OnPauseMenuRequest?.Invoke();
    }

    #endregion

    #region Managing general events

    public event Action<int> OnCursorChange;
    public event Action<int> OnEquipmentChange;
    public event Action<Vector3, Transform, FactionManager.Faction> OnCooperationRequest;
    public event Action<Transform, bool> OnNpcInteractionWithObject;
    public event Action<Transform, int> OnEnemyDeath;
    public event Action OnFactionsChange;
    public event Action<string, string, int, string> OnDisplayBannerBar;
    public event Action<int> OnDisplayCountdown;
    public event Action<string, NpcIdentifier> OnWaveEvent;

    // Used by MouseController to change cursors
    public void ChangeCursor(int cursor)
    {
      OnCursorChange?.Invoke(cursor);
    }

    // Used by PlayerController to inform EquipmentBar about equipment change
    public void ChangeEquipment(int slot)
    {
      OnEquipmentChange?.Invoke(slot);
    }

    // Used by MCooperation to process cooperation requests
    public void RequestCooperation(Vector3 position, Transform target,
      FactionManager.Faction faction)
    {
      OnCooperationRequest?.Invoke(position, target, faction);
    }

    // Used by QuestGiver to inform NPC state machine that an interaction has started
    // Note that the 'npc' parameter is the NPC that is interacting with the character, not the
    // character the NPC is interacting with. It is used to determine which NPC should change state. 
    public void InteractWithNearbyCharacter(Transform npc, bool started)
    {
      OnNpcInteractionWithObject?.Invoke(npc, started);
    }

    // Used by MExpBar to process any possible experience gained
    public void EnemyDies(Transform causedBy, int experience)
    {
      OnEnemyDeath?.Invoke(causedBy, experience);
    }

    // Used by various classes to request updated FactionGroups from FactionManager
    public void ChangeFactions()
    {
      OnFactionsChange?.Invoke();
    }

    // Used by in-game triggers to display the banner bar e.g. when changing area
    public void DisplayBannerBar(string title, string message, int duration,
      string audioClipName = "Bing")
    {
      OnDisplayBannerBar?.Invoke(title, message, duration, audioClipName);
    }

    // Used by in-game triggers to display use banner bar to display countdown,
    // for example, before starting an enemy wave event
    public void DisplayCountdown(int seconds = 3)
    {
      OnDisplayCountdown?.Invoke(seconds);
    }

    // Used by MEventDefeatable to send event wave data back to WaveEventManager
    public void TriggerWaveEvent(string eventId, NpcIdentifier npcId)
    {
      OnWaveEvent?.Invoke(eventId, npcId);
    }

    #endregion

    #region Managing quests & dialogue

    public event Action<TextAsset, string> OnDialogueTrigger;
    public event Action OnDialogueContinueTrigger;
    public event Action<bool> OnDialogueStateChange;
    public event Action<string> OnInteractionTriggerRadiusExit;
    public event Action<int> OnDialogueChoiceSubmission;
    public event Action<DialogueVariables> OnDialogueVariablesShare;
    public event Action<Quest, bool> OnQuestStateChange;

    // Used by QuestGiver to initiate/continue/stop dialogue
    public void TriggerDialogue(TextAsset inkJson, string speaker)
    {
      OnDialogueTrigger?.Invoke(inkJson, speaker);
    }

    // Used by DialogueUI to continue current dialogue via UI button click
    public void TriggerDialogueContinuation()
    {
      OnDialogueContinueTrigger?.Invoke();
    }

    // Used to get PlayerController to react on dialogue state by e.g. disable shooting,
    // hide EquipmentBar, etc.
    public void ChangeDialogueState(bool status)
    {
      OnDialogueStateChange?.Invoke(status);
    }

    // Used by buttons on the dialogueCanvas (e.g. the Dialogue-Choice-Button prefab
    // to inform DialogueManager about a dialogue choice that has been made
    public void SubmitDialogueChoice(int choice)
    {
      OnDialogueChoiceSubmission?.Invoke(choice);
    }

    // Used by MButtonInGame to notify DialogueManager that dialogue has to be paused
    // because player left the interactionRadius so the button is no longer visible
    // and the player can no longer interact with the button/object/NPC - it is also
    // used to notify the QuestManager _which_ story the event relates to, so that
    // all variables in the related quest can be updated
    public void ExitInteractionTriggerRadius(string nameOfStory)
    {
      OnInteractionTriggerRadiusExit?.Invoke(nameOfStory);
    }

    // Used to share global Ink dialogue variables between QuestManager and
    // DialogueManager which is required to track QuestState, progress, and more...
    public void ShareDialogueVariables(DialogueVariables variables)
    {
      OnDialogueVariablesShare?.Invoke(variables);
    }

    // Used to share updates to quests that are used by the MQuestTracker to display
    // and update active quests in the UI
    public void ChangeQuestState(Quest quest, bool onlyStateChange = false)
    {
      OnQuestStateChange?.Invoke(quest, onlyStateChange);
    }

    #endregion
  }
}