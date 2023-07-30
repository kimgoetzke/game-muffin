using CaptainHindsight.Core;
using CaptainHindsight.Core.StateMachine;
using UnityEngine;

namespace CaptainHindsight.NPCs.States
{
  public class Move : BState
  {
    #region State machine reference

    protected Npc Sm;

    public Move(Npc stateMachine) : base("Move", stateMachine)
    {
      Sm = (Npc)this.stateMachine;
    }

    #endregion

    private float _timer;
    private float _cooldown = 2f;

    #region State logic overrides

    public override void Enter()
    {
      base.Enter();

      Sm.stateLock = NpcStateLock.Partial;
      Sm.navMeshAgent.speed = Sm.actionSpeed;
      Sm.MoveAwayFromObject(false);
      Sm.SetAnimations(false, false, true, 2);
    }

    public override void UpdateLogic()
    {
      base.UpdateLogic();

      if (Sm.AgentHasReachedDestination())
      {
        _timer += Time.deltaTime;

        if (_timer >= _cooldown)
        {
          _timer = 0f;

          // Exit state if object is not close by
          if (Sm.ObjectIsClose() == false)
            Sm.SwitchToDefaultMovementState();
        }
      }
    }

    public override void UpdatePhysics()
    {
      base.UpdatePhysics();

      Sm.UpdateAnimationsAndRotation();

      if (Sm.ObjectIsClose()) Sm.MoveAwayFromObject(false);
    }

    public override void Exit()
    {
      base.Exit();

      Sm.stateLock = NpcStateLock.Off;
      Sm.navMeshAgent.SetDestination(Sm.transform.position);
      Sm.navMeshAgent.speed = Sm.movementSpeed;
      Sm.SetAnimations(false, false, true, 1);
    }

    #endregion
  }
}