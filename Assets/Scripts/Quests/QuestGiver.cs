using CaptainHindsight.Core;
using CaptainHindsight.Core.Observer;
using CaptainHindsight.Core.StateMachine;
using CaptainHindsight.Managers;
using CaptainHindsight.Other;
using CaptainHindsight.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Quests
{
  public class QuestGiver : NpcObserver, IInteractable
  {
    [SerializeField] [Required]
    private Transform indicatorTransform;

    [SerializeField] [PropertyRange(1, 10)] [OnValueChanged("UpdateInteractionRange")]
    private float interactionRange = 1;

    [SerializeField] [Required] private string speaker;

    private MButtonInGame _inGameUI;

    [SerializeField]
    [Required]
    [InlineEditor(InlineEditorObjectFieldModes.Foldout, Expanded = false)]
    private QuestData questData;

    [ShowInInspector, ReadOnly] private bool _isBlocked;

    #region Unity Editor methods (Odin and OnDrawGizmos)

    private void UpdateInteractionRange()
    {
      GetComponent<SphereCollider>().radius = interactionRange;
    }

    #endregion

    #region Awake, Start, and initialisation

    protected override void Awake()
    {
      base.Awake();
      _inGameUI = indicatorTransform.GetComponentInChildren<MButtonInGame>();
      _inGameUI.InitialiseButton(interactionRange, questData.story.name);
    }


    public override void ProcessInformation(BState state)
    {
      _isBlocked = state.name switch
      {
        "Attack" => true,
        _ => false
      };
    }

    #endregion

    public void Interact(Vector3 position)
    {
      if (_isBlocked)
      {
        Helper.Log("[QuestGiver] Request ignored. Interaction blocked by another NPC state.");
        return;
      }

      // TODO: Check interaction range (player's OverlapSphere is much larger to allow for
      // all sorts of interactions, not only with objects right in front of the player)
      var distance = (position - transform.position).magnitude;
      if (distance >= interactionRange + 0.25f) return;

      _inGameUI.Interact();
      EventManager.Instance.TriggerDialogue(questData.story, speaker);
      EventManager.Instance.InteractWithNearbyCharacter(transform, true);

      // TODO: When there's a use case, consider ending interaction. Currently, the NPC will remain in the
      // ObserveState until player/NPC die or NPC is attacked. At this point, there's no easy way to
      // implement this as only the MButtonInGame knows when the interaction is over and the game world
      // and UI should remain independent of each other.
    }
  }
}