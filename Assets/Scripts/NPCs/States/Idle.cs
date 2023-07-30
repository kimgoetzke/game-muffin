using CaptainHindsight.Core;
using CaptainHindsight.Core.StateMachine;

namespace CaptainHindsight.NPCs.States
{
  public class Idle : BState
  {
    #region State machine reference

    protected Npc Sm;

    public Idle(Npc stateMachine) : base("Idle", stateMachine)
    {
      Sm = (Npc)this.stateMachine;
    }

    #endregion

    #region State logic overrides

    public override void Enter()
    {
      base.Enter();

      Sm.RemoveCurrentTarget();

      switch (Sm.movement)
      {
        case NpcMovement.Wander:
          stateMachine.SwitchState(Sm.WanderState);
          break;
        case NpcMovement.Idle:
          break;
        case NpcMovement.Patrol:
          stateMachine.SwitchState(Sm.PatrolState);
          break;
        default:
          Sm.LogSwitchStateWarning(this);
          break;
      }
    }

    public override void UpdatePhysics()
    {
      base.UpdatePhysics();

      Sm.MoveIfPushedAway();
    }

    #endregion
  }
}