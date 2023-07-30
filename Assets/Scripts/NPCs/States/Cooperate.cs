using CaptainHindsight.Core;
using CaptainHindsight.Core.StateMachine;
using UnityEngine;

namespace CaptainHindsight.NPCs.States
{
  public class Cooperate : BState
  {
    #region State machine reference

    private readonly Npc _sm;

    public Cooperate(Npc stateMachine) : base("Cooperate", stateMachine)
    {
      _sm = (Npc)this.stateMachine;
    }

    #endregion

    private float _timer;

    #region State logic overrides

    public override void Enter()
    {
      base.Enter();

      _sm.SetAnimations(false, true);
    }

    public override void UpdateLogic()
    {
      base.UpdateLogic();

      _timer += Time.deltaTime;
      if (!(_timer >= 1f)) return;
      
      switch (_sm.behaviour)
      {
        case NpcBehaviour.Anxious:
          _sm.SwitchState(_sm.FleeState);
          break;
        case NpcBehaviour.Neutral:
          _sm.SwitchToDefaultMovementState();
          break;
        case NpcBehaviour.Aggressive:
          _sm.SwitchState(_sm.AttackState);
          break;
        case NpcBehaviour.Indifferent:
        case NpcBehaviour.Observant:
        case NpcBehaviour.Defensive:
        default:
          Helper.LogWarning("[Cooperate] SwitchState not implemented for " + _sm.behaviour + ".");
          _sm.LogSwitchStateWarning(this);
          _sm.SwitchToDefaultMovementState();
          break;
      }
    }

    public override void UpdatePhysics()
    {
      base.UpdatePhysics();

      _sm.FaceTarget();
    }

    public override void Exit()
    {
      base.Exit();

      _timer = 0f;

      _sm.SetAnimations(false, false);
    }

    #endregion
  }
}