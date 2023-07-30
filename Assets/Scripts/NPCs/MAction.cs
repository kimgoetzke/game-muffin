using CaptainHindsight.Core;
using CaptainHindsight.Player;
using Sirenix.OdinInspector;
using UnityEngine;
using static CaptainHindsight.Managers.FactionManager;

namespace CaptainHindsight.NPCs
{
  [DisallowMultipleComponent]
  [RequireComponent(typeof(SphereCollider))]
  public class MAction : MonoBehaviour
  {
    [Title("Configuration")] [SerializeField] [Required]
    private FactionIdentity factionIdentity;

    [Required] [SerializeField] [PropertyRange(0.1f, 5)] [OnValueChanged("UpdateActionRadius")]
    private float actionRadius = 1f;

    private Npc _npc;

    #region Unity Editor methods (Odin and OnDrawGizmos)

    private void UpdateActionRadius()
    {
      GetComponent<SphereCollider>().radius = actionRadius;
    }

    #endregion

    private void Start()
    {
      _npc = Helper.GetComponentSafely<Npc>(transform.parent.gameObject);
      if (_npc != null)
        _npc.InitialiseActionTrigger(actionRadius);
      else
        gameObject.SetActive(false);
      //Helper.LogWarning("[MAction] " + npc.transform.name + " couldn't find parent state machine. Initialisation incomplete. AwarenessRadius disabled.");
    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.CompareTag("Bullet"))
      {
        _npc.ActionTrigger(other.GetComponent<Bullet>().owner);
      }
      else if (other.CompareTag("NPC") || other.CompareTag("Player"))
      {
        var otherFaction = other.GetComponent<FactionIdentity>().GetFaction();
        var factionStatus = factionIdentity.GetMyFactionStatus(otherFaction);
        switch (factionStatus)
        {
          case FactionStatus.Neutral:
            //Helper.Log("[MAction] " + npc.transform.name + ": Neutral towards " + other.name + " - no action taken.");
            return;
          case FactionStatus.Ally:
            //Helper.Log("[MAction] " + npc.transform.name + ": Allied to " + other.name + " - no action taken.");
            return;
          default:
            break;
        }

        _npc.ActionTrigger(other.transform);
      }
      else
      {
        return;
      }

      //Helper.Log("[MAction] " + npc.transform.name + ": " + other.name + " entered the actionRadius.");
    }
  }
}