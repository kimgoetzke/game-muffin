using CaptainHindsight.Managers;
using CaptainHindsight.Other;
using CaptainHindsight.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Quests
{
  public class QuestGiver : MonoBehaviour, IInteractable
  {
    [Title("Configuration")] [SerializeField] [Required]
    private Transform indicatorTransform;

    [SerializeField] [PropertyRange(1, 10)] [OnValueChanged("UpdateInteractionRange")]
    private float interactionRange = 1;

    [SerializeField] [Required] private string speaker;

    private MButtonInGame _inGameUI;

    [SerializeField]
    [Required]
    [InlineEditor(InlineEditorObjectFieldModes.Foldout, Expanded = false)]
    private QuestData questData;

    #region Unity Editor methods (Odin and OnDrawGizmos)

    private void UpdateInteractionRange()
    {
      GetComponent<SphereCollider>().radius = interactionRange;
    }

    #endregion

    #region Awake, Start, and initialisation

    private void Awake()
    {
      _inGameUI = indicatorTransform.GetComponentInChildren<MButtonInGame>();
      _inGameUI.InitialiseButton(interactionRange, questData.story.name);
    }

    #endregion

    public void Interact(Vector3 position)
    {
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