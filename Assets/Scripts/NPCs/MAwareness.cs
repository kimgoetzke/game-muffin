using CaptainHindsight.Core;
using CaptainHindsight.Other;
using Sirenix.OdinInspector;
using UnityEngine;
using static CaptainHindsight.Managers.FactionManager;

namespace CaptainHindsight.NPCs
{
  [DisallowMultipleComponent]
  [RequireComponent(typeof(SphereCollider))]
  public class MAwareness : MonoBehaviour, INotice
  {
    [Title("Configuration")] [SerializeField] [Required]
    private FactionIdentity factionIdentity;

    [ShowInInspector] [ReadOnly] private Faction _myFaction = Faction.Unspecified;

    [SerializeField] [PropertyRange(1, 10)] [OnValueChanged("UpdateAwarenessRadius")]
    private float awarenessRadius = 1;

    private Npc _npc;

    #region Unity Editor methods (Odin and OnDrawGizmos)

    private void UpdateAwarenessRadius()
    {
      GetComponent<SphereCollider>().radius = awarenessRadius;
    }

    #endregion

    private void Start()
    {
      _myFaction = factionIdentity.GetFaction();
      _npc = Helper.GetComponentSafely<Npc>(transform.parent.gameObject);
      if (_npc != null)
      {
        var sphereCollider = GetComponent<SphereCollider>();
        _npc.InitialiseAwarenessTrigger(sphereCollider.center, sphereCollider.radius);
      }
      else
      {
        gameObject.SetActive(false);
        Helper.LogWarning("[MAwareness] " + _npc.transform.name +
                          " couldn't find parent state machine. Initialisation incomplete. AwarenessRadius disabled.");
      }
    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.CompareTag("Player") || other.CompareTag("NPC"))
      {
        var otherFaction = other.GetComponent<FactionIdentity>().GetFaction();
        var factionStatus = factionIdentity.GetMyFactionStatus(otherFaction);
        //Helper.Log("[MAwareness] " + npc.transform.name + " spotted " + other.transform.name + " (FactionStatus: " + factionStatus +").");
        _npc.AwarenessTrigger(true, other.transform, factionStatus);
        return;
      }
    }

    private void OnTriggerExit(Collider other)
    {
      if (other.CompareTag("Player") || other.CompareTag("NPC"))
      {
        //Helper.Log("[MAwareness] " + npc.transform.name + " lost sight of " + other.transform.name + ".");
        _npc.AwarenessTrigger(false, other.transform);
        return;
      }
    }

    public void Notice(Transform target)
    {
      if (target.CompareTag("Impact"))
      {
        //Helper.Log("[MAwareness] " + transform.parent.name + " noticed " + target.tag + " at " + target.position + ".");
        _npc.AwarenessTrigger(true, target);
        return;
      }
    }
  }
}