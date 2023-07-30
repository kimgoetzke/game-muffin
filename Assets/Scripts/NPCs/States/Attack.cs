using CaptainHindsight.Core;
using CaptainHindsight.Core.StateMachine;
using UnityEngine;

namespace CaptainHindsight.NPCs.States
{
  public class Attack : BState
  {
    #region State machine reference

    protected Npc Sm;

    public Attack(Npc stateMachine) : base("Attack", stateMachine)
    {
      Sm = (Npc)this.stateMachine;
    }

    #endregion

    private float _cooldown;
    private float _timer;
    private float _facingTargetTimer;
    private float _newTargetCooldown;

    #region State logic overrides

    public override void Enter()
    {
      base.Enter();

      Sm.navMeshAgent.SetDestination(Sm.currentTarget.position);
      Sm.stateLock = NpcStateLock.Full;
      Sm.navMeshAgent.speed = Sm.actionSpeed;
      Sm.ChangeAnimationLayer(1, 1);
      Sm.SetAnimations(false, false, true, 2);
    }

    public override void UpdatePhysics()
    {
      base.UpdatePhysics();

      if (_cooldown > 0)
      {
        _cooldown -= Time.deltaTime;
        return;
      }

      Sm.UpdateAnimationsAndRotation();

      if (Sm.ObjectIsClose())
      {
        //if (sm.NavMeshAgent.velocity.x == 0f) sm.FaceTarget();
        _facingTargetTimer += Time.deltaTime;
        if (_facingTargetTimer <= 0.5f)
        {
          Sm.FaceTarget();
          _facingTargetTimer = 0f;
        }

        Sm.navMeshAgent.SetDestination(Sm.transform.position);
        Sm.audioController.Play(AudioController.AudioType.Attack);
        Sm.animationController.SetTrigger("Attack", true, NpcAnimationTrigger.Attack);
        _cooldown = Sm.animationController.GetCurrentAnimatorStateInfo(0);
      }
      else if (Sm.isCooperating == false && Sm.ObjectIsFar())
      {
        _timer += Time.deltaTime;

        // Switch back to default movement state after (ActionFocus) seconds
        if (_timer >= Sm.actionFocus)
          Sm.SwitchToDefaultMovementState();

        // Check for new targets during this time
        if (_newTargetCooldown <= 0)
        {
          Sm.CheckForNewTarget();
          _newTargetCooldown = 0.75f;
        }

        _newTargetCooldown -= Time.deltaTime;
      }
      else
      {
        if (Sm.AgentHasReachedDestination())
        {
          Sm.isCooperating = false;
          if (Sm.currentTarget != null) Sm.navMeshAgent.SetDestination(Sm.currentTarget.position);
          else Sm.SwitchToDefaultMovementState();
        }
      }
    }

    public override void Exit()
    {
      base.Exit();

      _cooldown = 0f;
      _timer = 0f;
      _newTargetCooldown = 0f;
      Sm.isCooperating = false;
      Sm.stateLock = NpcStateLock.Off;
      Sm.ChangeAnimationLayer(1, 0);
      Sm.SetAnimations(false, false, true, 1);
      Sm.navMeshAgent.speed = Sm.movementSpeed;
      Sm.navMeshAgent.SetDestination(Sm.transform.position);
    }

    #endregion
  }
}