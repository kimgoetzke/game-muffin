using CaptainHindsight.Core;
using CaptainHindsight.Core.StateMachine;

namespace CaptainHindsight.NPCs.States
{
  public class Observe : BState
  {
    #region State machine reference

    protected Npc Sm;

    public Observe(Npc stateMachine) : base("Observe", stateMachine)
    {
      Sm = (Npc)this.stateMachine;
    }

    #endregion

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

      Sm.FaceTarget();
    }

    public override void Exit()
    {
      base.Exit();

      Sm.SetAnimations(false, false);
    }

    #endregion
  }
}