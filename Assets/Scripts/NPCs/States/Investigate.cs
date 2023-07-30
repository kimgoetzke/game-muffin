using CaptainHindsight.Core;
using CaptainHindsight.Core.StateMachine;
using UnityEngine;

namespace CaptainHindsight.NPCs.States
{
  public class Investigate : BState
  {
    #region State machine reference

    protected Npc Sm;

    public Investigate(Npc stateMachine) : base("Investigate", stateMachine)
    {
      Sm = (Npc)this.stateMachine;
    }

    #endregion

    private float _investigationTimer;
    private float _investigationCountdown;
    private float _switchStateTimer;
    private float _switchStateCountdown;
    private bool _isInvestigating;

    #region State logic overrides

    public override void Enter()
    {
      base.Enter();

      _investigationCountdown = Sm.investigateAfter;
      _switchStateCountdown = Mathf.Max(Sm.investigateAfter * 2, 2f);
      Sm.FaceTarget();
      Sm.stateLock = NpcStateLock.Partial;
      Sm.SetAnimations(false, true);
    }

    public override void UpdatePhysics()
    {
      base.UpdatePhysics();

      _investigationTimer += Time.deltaTime;

      if (_investigationTimer >= _investigationCountdown)
      {
        // Set new target if CurrentTarget is close enough
        if (Sm.ObjectIsFar() == false)
        {
          Sm.animationController.SetBool("isAware", false);
          Sm.navMeshAgent.SetDestination(Sm.currentTarget.position);
          _isInvestigating = true;
          _investigationTimer = 0f;
          _switchStateTimer = 0f;
          Helper.Log("[NPC] " + Sm.transform.name + ": SetDestination for investigation at " +
                     Sm.currentTarget.position + ".");
        }
        else
        {
          _isInvestigating = false;
        }

        // Initiate switchState if isInvestigating is false
        if (_isInvestigating == false && Sm.AgentHasReachedDestination())
        {
          if (Sm.navMeshAgent.velocity.magnitude > 0) return;

          _switchStateTimer += Time.deltaTime;

          if (Sm.animationController.GetBool("isAware") == false)
          {
            Helper.Log("[NPC] " + Sm.transform.name +
                       ": Investigation is over. CurrentTarget is too far away. SwitchState countdown has begun...");
            Sm.animationController.SetBool("isAware", true);
          }

          if (_switchStateTimer >= _switchStateCountdown)
            Sm.SwitchToDefaultMovementState();
        }
      }

      Sm.UpdateAnimationsAndRotation();
    }

    public override void Exit()
    {
      base.Exit();

      _investigationTimer = 0f;
      _switchStateTimer = 0f;
      _isInvestigating = false;
      Sm.stateLock = NpcStateLock.Off;
      Sm.SetAnimations(false, false);
      Sm.navMeshAgent.SetDestination(Sm.transform.position);
    }

    #endregion
  }
}