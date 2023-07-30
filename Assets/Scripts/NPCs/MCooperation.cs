using CaptainHindsight.Core;
using CaptainHindsight.Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using static CaptainHindsight.Managers.FactionManager;

namespace CaptainHindsight.NPCs
{
  [DisallowMultipleComponent]
  public class MCooperation : MonoBehaviour
  {
    [Title("Configuration")] [SerializeField] [Required]
    private FactionIdentity factionIdentity;

    [ShowInInspector] [ReadOnly] private Faction _myFaction = Faction.Unspecified;

    [SerializeField] [PropertyRange(2, 20)]
    private float cooperationRadius = 2;

    private Npc _npc;

    #region Unity Editor methods (Odin and OnDrawGizmos)

    private void OnDrawGizmosSelected()
    {
      Gizmos.color = Color.blue;
      Gizmos.DrawWireSphere(transform.position, cooperationRadius);
    }

    #endregion

    private void Start()
    {
      _myFaction = factionIdentity.GetFaction();
      _npc = Helper.GetComponentSafely<Npc>(transform.parent.gameObject);
      if (_npc == null)
        Helper.LogWarning("[MCooperation] " + _npc.transform.name +
                          ": Cooperation was initiated but NPC state machine not found. Request incomplete.");
    }

    private void ActionCooperationRequest(Vector3 allyPosition, Transform target, Faction faction)
    {
      // Only listen to request from same faction
      // IMPORTANT: This may need to be reworked if there are cross-non-player-faction alliances
      if (faction != _myFaction) return;

      // Only listen to request if distance is within cooperationRadius
      var distance = Vector3.Distance(allyPosition, transform.position);
      if (distance <= 0.1f || distance >= cooperationRadius) return;

      // Action request
      if (_npc != null) _npc.CooperateWithOthers(target);
      //Helper.Log("[MCooperation] " + transform.parent.name + " will cooperate (distance: " + distance + ").");
    }

    #region Managing events

    private void OnEnable()
    {
      EventManager.Instance.OnCooperationRequest += ActionCooperationRequest;
    }

    private void OnDestroy()
    {
      EventManager.Instance.OnCooperationRequest -= ActionCooperationRequest;
    }

    #endregion
  }
}