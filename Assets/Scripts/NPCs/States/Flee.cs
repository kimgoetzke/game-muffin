using CaptainHindsight.Core;
using CaptainHindsight.Core.StateMachine;

namespace CaptainHindsight.NPCs.States
{
  public class Flee : BState
  {
    #region State machine reference

    protected Npc Sm;

    public Flee(Npc stateMachine) : base("Flee", stateMachine)
    {
      Sm = (Npc)this.stateMachine;
    }

    #endregion

    #region State logic overrides

    public override void Enter()
    {
      base.Enter();

      Sm.stateLock = NpcStateLock.Full;
      Sm.navMeshAgent.speed = Sm.actionSpeed;
      Sm.SetAnimations(false, false, true, 2);

      // Set destination for fleeing based on settings
      if (Sm.randomFlee) Sm.MoveAwayFromObject(true);
      else Sm.navMeshAgent.SetDestination(Sm.defaultTarget.position);
    }

    public override void UpdateLogic()
    {
      base.UpdateLogic();

      Sm.UpdateAnimationsAndRotation();

      if (Sm.AgentHasReachedDestination())
        Sm.SwitchToDefaultMovementState();
    }

    public override void Exit()
    {
      base.Exit();

      Sm.stateLock = NpcStateLock.Off;
      Sm.SetAnimations(false, false, true, 1);
      Sm.navMeshAgent.SetDestination(Sm.transform.position);
      Sm.navMeshAgent.speed = Sm.movementSpeed;
    }

    #endregion
  }
}