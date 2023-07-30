using CaptainHindsight.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CaptainHindsight.Managers
{
  public class FactionManager : MonoBehaviour
  {
    public static FactionManager Instance;

    [FormerlySerializedAs("RIGID")]
    [SerializeField]
    [BoxGroup("RIGID")]
    [HideLabel]
    [PropertySpace(SpaceAfter = 10)]
    private FactionGroup rigid;

    [FormerlySerializedAs("CorpX")]
    [SerializeField]
    [BoxGroup("CorpX")]
    [HideLabel]
    [PropertySpace(SpaceAfter = 10)]
    private FactionGroup corpX;

    [FormerlySerializedAs("Dinosaur")]
    [SerializeField]
    [BoxGroup("Dinosaur")]
    [HideLabel]
    [PropertySpace(SpaceAfter = 10)]
    private FactionGroup dinosaur;

    [FormerlySerializedAs("Unspecified")]
    [SerializeField]
    [BoxGroup("Other")]
    [HideLabel]
    [PropertySpace(SpaceAfter = 10)]
    private FactionGroup unspecified;

    [HideInInspector] private bool _firstCallBlocked;

    #region Unity Editor methods

    private void OnValidate()
    {
      if (Application.isPlaying)
      {
        if (_firstCallBlocked == false)
        {
          _firstCallBlocked = true;
          return;
        }

        EventManager.Instance.ChangeFactions();
        Helper.Log(
          "[FactionManager] FactionsGroup(s) have/has changed during playmode so all instances of FactionIdentity were updated.");
      }
    }

    #endregion

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
    }

    public FactionGroup GetFactionGroup(Faction faction)
    {
      FactionGroup group;
      switch (faction)
      {
        case Faction.Rigid:
          group = rigid;
          break;
        case Faction.CorpX:
          group = corpX;
          break;
        case Faction.Dinosaur:
          group = dinosaur;
          break;
        default:
          group = unspecified;
          break;
      }

      return group;
    }

    [System.Serializable]
    public class FactionGroup
    {
      [FormerlySerializedAs("Player")] [EnumToggleButtons] [LabelWidth(75)]
      public FactionStatus player;

      [FormerlySerializedAs("Dinosaur")] [EnumToggleButtons] [LabelWidth(75)]
      public FactionStatus dinosaur;

      [FormerlySerializedAs("RIGID")] [EnumToggleButtons] [LabelWidth(75)]
      public FactionStatus rigid;

      [FormerlySerializedAs("CorpX")] [EnumToggleButtons] [LabelWidth(75)]
      public FactionStatus corpX;

      [FormerlySerializedAs("Unspecified")] [EnumToggleButtons] [LabelWidth(75)]
      public FactionStatus unspecified;
    }

    public enum Faction
    {
      Unspecified,
      Player,
      Dinosaur,
      Rigid,
      CorpX
    }

    public enum FactionStatus
    {
      Hostile,
      Neutral,
      Ally,
      None
    }
  }
}