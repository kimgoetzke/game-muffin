using CaptainHindsight.Core;
using CaptainHindsight.Core.StateMachine;

namespace CaptainHindsight.NPCs.States
{
  public class Watch : BState
  {
    #region State machine reference

    protected Npc Sm;

    public Watch(Npc stateMachine) : base("Watch", stateMachine)
    {
      Sm = (Npc)this.stateMachine;
    }

    #endregion

    private float _countdown;

    #region State logic overrides

    public override void Enter()
    {
      base.Enter();

      Sm.SetAnimations(false, true);
    }

    public override void UpdateLogic()
    {
      base.UpdateLogic();

      if (Sm.currentTarget == null && Sm.stateLock == NpcStateLock.Off)
        Sm.SwitchToDefaultMovementState();
    }

    public override void UpdatePhysics()
    {
      base.UpdatePhysics();

      if (Sm.navMeshAgent.velocity.x != 0 || Sm.navMeshAgent.velocity.y != 0)
      {
        _countdown = 0.5f;
        Sm.UpdateAnimationsAndRotation();
      }
      else if (_countdown > 0f)
      {
        Sm.UpdateAnimationsAndRotation();
      }
      else
      {
        Sm.FaceTarget();
      }
    }

    public override void Exit()
    {
      base.Exit();

      _countdown = 0f;
      Sm.SetAnimations(false, false);
    }

    #endregion
  }
}