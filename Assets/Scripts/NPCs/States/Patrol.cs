using CaptainHindsight.Core.StateMachine;
using UnityEngine;

namespace CaptainHindsight.NPCs.States
{
  public class Patrol : BState
  {
    #region State machine reference

    protected Npc Sm;

    public Patrol(Npc stateMachine) : base("Patrol", stateMachine)
    {
      Sm = (Npc)this.stateMachine;
    }

    #endregion

    private float _timer;
    private bool _isWaiting;
    private int _currentWaypointIndex;

    #region State logic overrides

    public override void Enter()
    {
      base.Enter();

      Sm.RemoveCurrentTarget();
      Sm.navMeshAgent.SetDestination(Sm.waypoints[_currentWaypointIndex].position);
    }

    public override void UpdateLogic()
    {
      base.UpdateLogic();

      if (_isWaiting)
      {
        _timer += Time.deltaTime;
        if (_timer >= Sm.waitAtCheckPoint)
        {
          _isWaiting = false;
          Sm.navMeshAgent.SetDestination(Sm.waypoints[_currentWaypointIndex].position);
        }
      }
      else if (Sm.AgentHasReachedDestination())
      {
        _currentWaypointIndex = (_currentWaypointIndex + 1) % Sm.waypoints.Length;
        _timer = 0f;
        _isWaiting = true;
      }

      Sm.UpdateAnimationsAndRotation();
    }

    public override void Exit()
    {
      base.Exit();

      Sm.SetAnimations(false, false);
      Sm.navMeshAgent.SetDestination(Sm.transform.position);
    }

    #endregion
  }
}