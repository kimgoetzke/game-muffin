using CaptainHindsight.Core.StateMachine;
using UnityEngine;
using UnityEngine.AI;

namespace CaptainHindsight.NPCs.States
{
  public class Wander : BState
  {
    #region State machine reference

    protected Npc Sm;

    public Wander(Npc stateMachine) : base("Wander", stateMachine)
    {
      Sm = (Npc)this.stateMachine;
    }

    #endregion

    [HideInInspector] private float _timer;

    #region State logic overrides

    public override void Enter()
    {
      base.Enter();

      Sm.RemoveCurrentTarget();
    }

    public override void UpdateLogic()
    {
      base.UpdateLogic();

      _timer += Time.deltaTime;

      if (_timer >= Sm.wanderTimer)
      {
        var newPos = SetRandomPosition(Sm.transform.position, Sm.wanderRadius, -1);
        Sm.navMeshAgent.SetDestination(newPos);
        _timer = 0;
      }

      Sm.UpdateAnimationsAndRotation();
    }

    public override void Exit()
    {
      base.Exit();

      Sm.navMeshAgent.SetDestination(Sm.transform.position);
      Sm.SetAnimations(false, false);
    }

    #endregion

    #region State specific logic

    private static Vector3 SetRandomPosition(Vector3 origin, float dist, int layermask)
    {
      var randomDirection = Random.insideUnitSphere * dist;
      randomDirection += origin;
      NavMeshHit navMeshHit;

      // Note: This function currently targets all layers, not just the ground layer.
      // As a result, the destination can be set to a position outside the navMesh.
      // This is unlikely to be a problem but may look odd at times when the enemy
      // just stands infront of a cliff awkwardly.
      NavMesh.SamplePosition(randomDirection, out navMeshHit, dist, layermask);

      return navMeshHit.position;
    }

    #endregion
  }
}