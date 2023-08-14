using CaptainHindsight.Core;
using CaptainHindsight.Managers;
using CaptainHindsight.Quests;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.UI
{
  public class MDialogueTrigger : MonoBehaviour
  {
    [InfoBox(
      "Set 'Trigger Dialogue' to true if this component attached to an in-game button that can be triggered by mouse. IMPORTANT: Requires Quest Giver to be set.")]
    [SerializeField]
    private bool isOnQuestGiver;

    [SerializeField] [HideIf("isOnQuestGiver")]
    private QuestGiver questGiver;

    private void Awake()
    {
      if (isOnQuestGiver && questGiver == null)
        Helper.LogError(
          "[MDialogueTrigger] This component is set to trigger a dialogue but doesn't have a reference to the QuestGiver. Please fix!");
    }

    // This method is used on dialogue UI buttons in order to progress the dialogue
    // via the mouse (vs keyboard).
    public void ContinueDialogue()
    {
      EventManager.Instance.TriggerDialogueContinuation();
    }

    // This method is used by the Indicator prefab which is attached to the Interactable
    // game object of an NPC. It allows the in-game button to interact with an NPC to be
    // triggered by the mouse.
    public void TriggerDialogueViaQuestGiver()
    {
      questGiver.Interact(transform.position);
    }
  }
}