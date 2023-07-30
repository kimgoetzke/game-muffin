using CaptainHindsight.Core;
using CaptainHindsight.Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using static CaptainHindsight.Managers.FactionManager;

namespace CaptainHindsight.NPCs
{
  [DisallowMultipleComponent]
  public class FactionIdentity : MonoBehaviour
  {
    [SerializeField] private Faction faction;

    [ShowInInspector] [ReadOnly] [HideIf("faction", Faction.Player)]
    private FactionGroup _factionGroup;

    private void Start()
    {
      GetMyFactionGroup();
    }

    public Faction GetFaction()
    {
      return faction;
    }

    public void GetMyFactionGroup()
    {
      _factionGroup = Instance.GetFactionGroup(faction);
    }

    public FactionStatus GetMyFactionStatus(Faction otherFaction)
    {
      if (faction == Faction.Player)
      {
        Helper.LogWarning(
          "[FactionIdentity] Player cannot request faction status. The player's actions will determine the status.");
        return FactionStatus.None;
      }

      FactionStatus factionStatus;
      switch (otherFaction)
      {
        case Faction.Player:
          factionStatus = _factionGroup.player;
          break;
        case Faction.Dinosaur:
          factionStatus = _factionGroup.dinosaur;
          break;
        case Faction.Rigid:
          factionStatus = _factionGroup.rigid;
          break;
        case Faction.CorpX:
          factionStatus = _factionGroup.corpX;
          break;
        default:
          factionStatus = _factionGroup.unspecified;
          break;
      }

      return factionStatus;
    }

    private void ActionFactionChange()
    {
      //Helper.Log("[FactionIdentity] " + transform.name + " requested updated FactionGroup.");
      GetMyFactionGroup();
    }

    #region Managing events

    private void OnEnable()
    {
      EventManager.Instance.OnFactionsChange += ActionFactionChange;
    }

    private void OnDestroy()
    {
      EventManager.Instance.OnFactionsChange -= ActionFactionChange;
    }

    #endregion
  }
}