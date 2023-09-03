using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CaptainHindsight.Core;
using CaptainHindsight.Core.StateMachine;
using CaptainHindsight.Managers;
using CaptainHindsight.NPCs.AnimationControllers;
using CaptainHindsight.NPCs.States;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using static CaptainHindsight.Managers.FactionManager;

namespace CaptainHindsight.NPCs
{
  [DisallowMultipleComponent]
  public class Npc : BStateMachine
  {
    #region Defining variables

    // General variables
    [FoldoutGroup("General", false)] [ReadOnly]
    private NpcType _type; // Set by choosing an AnimationController type

    [FoldoutGroup("General", false)]
    [SerializeField]
    [ChildGameObjectsOnly(IncludeSelf = false), Required]
    private FactionIdentity factionIdentity;

    [FoldoutGroup("General", false)] [ShowInInspector] [ReadOnly]
    private Faction _faction = Faction.Unspecified;

    [FormerlySerializedAs("AnimationController")]
    [FoldoutGroup("General", false)]
    [ChildGameObjectsOnly(IncludeSelf = true), Required]
    public AnimationController animationController;

    [FoldoutGroup("General", false)] [ChildGameObjectsOnly(IncludeSelf = true), Required]
    public AudioController audioController;

    [FormerlySerializedAs("NavMeshAgent")]
    [FoldoutGroup("General", false)]
    [ChildGameObjectsOnly(IncludeSelf = true)]
    [Required]
    public NavMeshAgent navMeshAgent;

    [FormerlySerializedAs("IsCooperating")]
    [FoldoutGroup("General", false)]
    [ShowInInspector]
    [ReadOnly]
    public bool isCooperating;

    [FoldoutGroup("General", false)] private float _countdown;

    // Awareness variables
    [FoldoutGroup("Awareness", false)] [ShowInInspector] [ReadOnly]
    private float _awarenessRadius;

    [FoldoutGroup("Awareness", false)] [ShowInInspector] [ReadOnly]
    private Vector3 _awarenessColliderCenter;

    [FoldoutGroup("Awareness", false)]
    [SerializeField]
    [Tooltip(
      "Determines how quickly NPC reacts to awarenessTriggers by adding a random delay with a minimum of this value.")]
    private float awarenessDelayMin;

    [FoldoutGroup("Awareness", false)]
    [SerializeField]
    [Tooltip(
      "Determines how quickly NPC reacts to awarenessTriggers by adding a random delay with a maximum of this value.")]
    private float awarenessDelayMax = 0.3f;

    [FoldoutGroup("Awareness", false)]
    [Tooltip(
      "Determines how quickly NPC reacts to awarenessTriggers by adding a random delay with a maximum of this value.")]
    [ShowInInspector]
    [ReadOnly]
    private float _awarenessDelay;

    [FoldoutGroup("Awareness", false)]
    [Tooltip(
      "Used in CheckForNewTargets() after CurrentTarget died/disappeared to make sure that NPC reacts quickly to nearby threats that were ignored due to StateLock.")]
    [SerializeField]
    private LayerMask searchLayers;

    // Action variables
    [FormerlySerializedAs("StateLock")]
    [BoxGroup("Action", centerLabel: true)]
    [ShowInInspector]
    [ReadOnly]
    public NpcStateLock stateLock = NpcStateLock.Off;

    [FormerlySerializedAs("CurrentTarget")]
    [BoxGroup("Action", centerLabel: true)]
    [ShowInInspector]
    [ReadOnly]
    public Transform currentTarget;

    [FormerlySerializedAs("CurrentTargetTag")]
    [BoxGroup("Action", centerLabel: true)]
    [ShowInInspector]
    [ReadOnly]
    public string currentTargetTag;

    [BoxGroup("Action", centerLabel: true)]
    [SerializeField]
    [Required]
    [PropertyRange(0.1f, 5)]
    [Tooltip(
      "Distance (in world units) between CurrentTarget and enemy for enemy to start attacking. The default is 1.")]
    private float actionDistance = 1f;

    [FormerlySerializedAs("ActionFocus")]
    [BoxGroup("Action", centerLabel: true)]
    [Required]
    [PropertyRange(1, 10)]
    [Tooltip(
      "Used in the attack state and defines the length of time the CurrentTarget has to be outside the Awareness Radius before returning to the default movement state.")]
    public float actionFocus = 2f;

    // Behaviour variables
    [FormerlySerializedAs("Behaviour")]
    [BoxGroup("Behaviour", centerLabel: true)]
    [GUIColor(0.3f, 0.8f, 0.8f)]
    [SerializeField]
    [Required]
    public NpcBehaviour behaviour = NpcBehaviour.Aggressive;

    [BoxGroup("Behaviour", centerLabel: true)] [ShowInInspector] [ReadOnly]
    private float _actionRadius;

    [FormerlySerializedAs("ActionSpeed")]
    [BoxGroup("Behaviour", centerLabel: true)]
    [Required]
    [SerializeField]
    [PropertyRange(1, 5)]
    public float actionSpeed = 2f;

    // - Aggressive
    [FormerlySerializedAs("InvestigateAfter")]
    [BoxGroup("Behaviour", centerLabel: true)]
    [Title("", "Aggressive", horizontalLine: false)]
    [PropertySpace(SpaceBefore = -15)]
    [ShowIf("behaviour", NpcBehaviour.Aggressive)]
    [SerializeField]
    public float investigateAfter = 2f;

    // - Anxious
    [FormerlySerializedAs("RandomFlee")]
    [BoxGroup("Behaviour", centerLabel: true)]
    [Title("", "Anxious", horizontalLine: false)]
    [PropertySpace(SpaceBefore = -15, SpaceAfter = 0)]
    [ShowIf("behaviour", NpcBehaviour.Anxious)]
    public bool randomFlee = true;

    [FormerlySerializedAs("FleeDistance")]
    [BoxGroup("Behaviour", centerLabel: true)]
    [ShowIf("@this.randomFlee && this.behaviour == NpcBehaviour.Anxious")]
    [PropertySpace(SpaceBefore = 0)]
    public float fleeDistance = 5f;

    [FormerlySerializedAs("DefaultTarget")]
    [BoxGroup("Behaviour", centerLabel: true)]
    [HideIf("@this.randomFlee || this.behaviour != NpcBehaviour.Anxious")]
    [PropertySpace(SpaceBefore = 0)]
    public Transform defaultTarget;

    // Movement variables
    [FormerlySerializedAs("Movement")]
    [BoxGroup("Movement", centerLabel: true)]
    [GUIColor(0.3f, 0.8f, 0.8f)]
    [SerializeField]
    [Required]
    public NpcMovement movement = NpcMovement.Wander;

    [FormerlySerializedAs("MovementSpeed")]
    [BoxGroup("Movement", centerLabel: true)]
    [Required]
    [SerializeField]
    [PropertyRange(0.1f, 5)]
    [OnValueChanged("UpdateMovementSpeed")]
    public float movementSpeed = 1f;

    // - Wandering
    [FormerlySerializedAs("WanderRadius")]
    [BoxGroup("Movement", centerLabel: true)]
    [Title("", "Wandering", horizontalLine: false)]
    [PropertySpace(SpaceBefore = -15)]
    [ShowIf("movement", NpcMovement.Wander)]
    [SerializeField]
    [PropertyRange(1, 10)]
    public float wanderRadius = 6f;

    [FormerlySerializedAs("WanderTimer")]
    [BoxGroup("Movement", centerLabel: true)]
    [ShowIf("movement", NpcMovement.Wander)]
    [SerializeField]
    [PropertyRange(1, 10)]
    public float wanderTimer = 2f;

    // - Patrolling
    [FormerlySerializedAs("Waypoints")]
    [BoxGroup("Movement", centerLabel: true)]
    [Title("", "Patrolling", horizontalLine: false)]
    [PropertySpace(SpaceBefore = -15)]
    [ShowIf("movement", NpcMovement.Patrol)]
    public Transform[] waypoints;

    [FormerlySerializedAs("WaitAtCheckPoint")]
    [BoxGroup("Movement", centerLabel: true)]
    [ShowIf("movement", NpcMovement.Patrol)]
    [SerializeField]
    [PropertyRange(0, 15)]
    public float waitAtCheckPoint = 1f;

    #region Unity Editor methods to visualise variables

    private void UpdateMovementSpeed()
    {
      GetComponent<NavMeshAgent>().speed = movementSpeed;
    }

    private void OnDrawGizmosSelected()
    {
      Gizmos.color = Color.yellow;
      Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }

    #endregion

    #endregion

    #region Set and initialise states and types

    // - Default movement behaviour i.e no CurrentTarget / is unaware
    [HideInInspector] public Idle IdleState;
    [HideInInspector] public Wander WanderState;

    [HideInInspector] public Patrol PatrolState;

    // - Default awareness behaviour i.e. has CurrentTarget / is aware
    [HideInInspector] public Observe ObserveState;
    [HideInInspector] public Investigate InvestigateState; // NPCStateLock.Partial

    [HideInInspector] public Cooperate CooperateState;

    // - Default action behaviour i.e. has CurrentTarget / is taking action
    [HideInInspector] public Watch WatchState; // No NPCStateLock
    [HideInInspector] public Flee FleeState; // NPCStateLock.Full
    [HideInInspector] public Move MoveState; // NPCStateLock.Partial
    [HideInInspector] public Attack AttackState; // NPCStateLock.Full

    private void Awake()
    {
      IdleState = new Idle(this);
      WanderState = new Wander(this);
      PatrolState = new Patrol(this);
      WatchState = new Watch(this);
      ObserveState = new Observe(this);
      InvestigateState = new Investigate(this);
      CooperateState = new Cooperate(this);
      FleeState = new Flee(this);
      MoveState = new Move(this);
      AttackState = new Attack(this);

      _faction = factionIdentity.GetFaction();
      _type = animationController.GetNpcType();
      _awarenessDelay = Random.Range(awarenessDelayMin, awarenessDelayMax);
    }

    #endregion

    protected override BState GetInitialState()
    {
      return IdleState;
    }

    public void LogSwitchStateWarning(object obj)
    {
      Helper.LogWarning("[NPC] Unknown state transition triggered (" + obj + ").");
    }

    #region Managing StateMachine wide methods to update direction/animation

    public void UpdateAnimationsAndRotation()
    {
      animationController.UpdateAnimationAndRotation(navMeshAgent.velocity.x,
        navMeshAgent.velocity.y);
    }

    public void FaceTarget()
    {
      animationController.FaceTarget(currentTarget);
    }

    public void SetAnimations(bool running, bool aware, bool speed = false, float speedValue = 0)
    {
      switch (_type)
      {
        case NpcType.Humanoid:
          animationController.SetFloat("moveSpeed", 0);
          break;
        case NpcType.Dinosaurs:
          animationController.SetBool("isRunning", running);
          animationController.SetBool("isAware", aware);
          if (speed) animationController.SetAnimatorSpeed(speedValue);
          break;
        case NpcType.Unspecified:
          break;
        default:
          break;
      }
    }

    public void ChangeAnimationLayer(int layer, float weight)
    {
      switch (_type)
      {
        case NpcType.Humanoid:
          animationController.ChangeAnimationLayer(layer, weight);
          break;
        case NpcType.Dinosaurs:
          break;
        case NpcType.Unspecified:
          break;
        default:
          break;
      }
    }

    #endregion

    #region Managing StateMachine wide methods to control logic

    private void SetCurrentTarget(Transform target)
    {
      currentTarget = target;
      currentTargetTag = target.tag;
    }

    public void RemoveCurrentTarget()
    {
      currentTarget = null;
      currentTargetTag = null;
    }

    private bool StatusOfCurrentTarget()
    {
      if (currentTarget == null) return false;
      else return true;
    }

    public bool ObjectIsClose()
    {
      if (StatusOfCurrentTarget() == false) return false;

      if (Vector3.Distance(currentTarget.position, transform.position) <= actionDistance)
        return true;
      else return false;
    }

    public bool ObjectIsFar()
    {
      if (StatusOfCurrentTarget() == false) return true;

      // Recalculate collider center (required due to localScale -1f)
      float offset;
      if (animationController.isFacingRight) offset = _awarenessColliderCenter.x * -1f;
      else offset = _awarenessColliderCenter.x;
      var colliderCenter = new Vector3(transform.position.x + offset, transform.position.y,
        transform.position.z);

      // Return true/false
      if (Vector3.Distance(colliderCenter, currentTarget.position) >
          _awarenessRadius + 0.5f) return true;
      else return false;
    }

    public bool AgentHasReachedDestination()
    {
      // If a path is pending i.e. being calculated, the destination
      // has not beeen reached yet
      if (navMeshAgent.pathPending) return false;

      // If the remaining distance is <= to the stopping distance, the
      // destination has been reached
      if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) return true;
      else return false;
    }

    public void CheckForNewTarget()
    {
      if (searchLayers == 0)
      {
        Helper.Log(
          "[NPC] Attempted to CheckForNewTarget() but there are no searchLayers specified.");
        return;
      }

      // Detect all colliders within awarenessRadius
      var colliders =
        Physics.OverlapSphere(transform.TransformPoint(_awarenessColliderCenter), _awarenessRadius,
          searchLayers);
      // Helper.Log("[NPC] Detected " + colliders.Length + " colliders in CheckForNewTargets().");

      // Guard clause and create variables
      if (colliders.Length == 0) return;
      var distance = Mathf.Infinity;
      var list = new List<SearchTargets>();

      // Calculate distance to this NPC and add to list if >0
      foreach (var c in colliders)
      {
        distance = (c.transform.position - transform.position).sqrMagnitude;
        if (distance != 0f)
        {
          var target = new SearchTargets { Collider = c, Distance = distance };
          list.Add(target);
          //Helper.Log("[NPC] Added collider " + colliders[i] + " at distance " + distance + " to the list.");
        }
      }

      // Sort the list (closest object first)
      list.Sort((x, y) => x.Distance.CompareTo(y.Distance));

      // For debugging
      // Helper.Log("[NPC] Final, sorted list:");
      // foreach (var item in list) Helper.Log(" - " + item.collider + " / " + item.distance + ".");

      // Reduce StateLock level to be able to take immediate action
      stateLock = NpcStateLock.Partial;

      // Go through list, starting with closest object, and determine faction, then
      // use ActionTrigger or AwarenessTrigger accordingly
      // 1. The loop will be broken when ActionTrigger changes StateLock.Full
      // 2. This currently assumes some form of "rage" as hostile targets will
      //    be attacked even if they are outside the actionRadius
      for (var i = 0; i < list.Count; i++)
      {
        if (stateLock == NpcStateLock.Full) return;
        var otherFaction = list[i].Collider.GetComponent<FactionIdentity>().GetFaction();
        var factionStatus = factionIdentity.GetMyFactionStatus(otherFaction);
        if (factionStatus == FactionStatus.Hostile)
          ActionTrigger(list[i].Collider.transform);
        else
          AwarenessTrigger(true, list[i].Collider.transform, factionStatus);
      }
    }

    private class SearchTargets
    {
      public Collider Collider;
      public float Distance;
    }

    #endregion

    #region Managing StateMachine wide methods to control movement

    public void SwitchToDefaultMovementState()
    {
      switch (movement)
      {
        case NpcMovement.Idle:
          SwitchState(IdleState);
          break;
        case NpcMovement.Wander:
          SwitchState(WanderState);
          break;
        case NpcMovement.Patrol:
          SwitchState(PatrolState);
          break;
        default:
          LogSwitchStateWarning(this);
          break;
      }
    }

    public void MoveAwayFromObject(bool isFleeing)
    {
      var direction = (currentTarget.position - transform.position).normalized;
      if (isFleeing)
        navMeshAgent.SetDestination(transform.TransformPoint(-direction * fleeDistance));
      else navMeshAgent.SetDestination(transform.TransformPoint(-direction * _actionRadius));

      // TODO: Add logic to calculate if proposed path is possible and if not, maybe go around player?

      // For debugging:
      // Debug.DrawLine(sm.transform.position, sm.transform.TransformPoint(direction * sm.ActionRadius), color: Color.red, 10f);
      // Debug.DrawLine(sm.transform.position, sm.transform.TransformPoint(-direction * sm.ActionRadius), color: Color.green, 10f);
    }

    public void MoveIfPushedAway()
    {
      if (navMeshAgent.velocity.sqrMagnitude > 0)
      {
        UpdateAnimationsAndRotation();
        _countdown = 0.5f;
      }
      else if (_countdown > 0f)
      {
        UpdateAnimationsAndRotation();
      }
    }

    #endregion

    #region Managing external methods used outside the state machine

    public void Die()
    {
      audioController.Play(AudioController.AudioType.Death);
      animationController.ChangeAnimationLayer(0, 1, true);
      animationController.ResetBools();
      animationController.SetTrigger("isDying");
      navMeshAgent.enabled = false;
      enabled = false;
      Destroy(this, 15f);
    }

    #endregion

    #region Awareness trigger

    public async void AwarenessTrigger(bool detected, Transform target,
      FactionStatus factionStatus = FactionStatus.None)
    {
      if (stateLock == NpcStateLock.Full || isCooperating ||
          factionStatus != FactionStatus.Hostile) return;

      await Task.Delay(System.TimeSpan.FromSeconds(_awarenessDelay));

      if (detected)
      {
        SetCurrentTarget(target);
        switch (behaviour)
        {
          case NpcBehaviour.Anxious:
            SwitchState(ObserveState);
            break;
          case NpcBehaviour.Neutral:
            SwitchState(ObserveState);
            break;
          case NpcBehaviour.Indifferent:
            SwitchState(IdleState);
            break;
          case NpcBehaviour.Observant:
            SwitchState(ObserveState);
            break;
          case NpcBehaviour.Defensive:
            SwitchState(ObserveState);
            break;
          case NpcBehaviour.Aggressive:
            SwitchState(InvestigateState);
            break;
          default:
            LogSwitchStateWarning(this);
            break;
        }
      }
      else
      {
        RemoveCurrentTarget();
      }
    }

    public void InitialiseAwarenessTrigger(Vector3 center, float radius)
    {
      _awarenessColliderCenter = center;
      _awarenessRadius = radius;
    }

    #endregion

    #region Action trigger

    public void ActionTrigger(Transform target)
    {
      // Guard clause - Only take action if StateLock is Partial or Off
      if (stateLock == NpcStateLock.Full) return;

      SetCurrentTarget(target);
      TakeAction();
    }

    private void TakeAction()
    {
      switch (behaviour)
      {
        case NpcBehaviour.Anxious:
          SwitchState(FleeState);
          break;
        case NpcBehaviour.Neutral:
          SwitchState(MoveState);
          break;
        case NpcBehaviour.Indifferent:
          SwitchState(IdleState);
          break;
        case NpcBehaviour.Observant:
          SwitchState(WatchState);
          break;
        case NpcBehaviour.Defensive:
          SwitchState(AttackState);
          break;
        case NpcBehaviour.Aggressive:
          RequestCooperation();
          SwitchState(AttackState);
          break;
        default:
          LogSwitchStateWarning(this);
          break;
      }
    }

    public void TakeActionAfterReceivingDamage(Transform target)
    {
      // Guard clause - Only take action if StateLock is Partial or Off
      if (stateLock == NpcStateLock.Full) return;

      Helper.Log("[NPC] " + transform.name + ": Takes action after receiving damage from " +
                 target.name + ".");

      SetCurrentTarget(target);
      switch (behaviour)
      {
        case NpcBehaviour.Anxious:
          SwitchState(FleeState);
          break;
        case NpcBehaviour.Neutral:
          SwitchState(FleeState);
          break;
        case NpcBehaviour.Indifferent:
          SwitchState(AttackState);
          break;
        case NpcBehaviour.Observant:
          SwitchState(FleeState);
          break;
        case NpcBehaviour.Defensive:
          SwitchState(AttackState);
          break;
        case NpcBehaviour.Aggressive:
          RequestCooperation();
          SwitchState(AttackState);
          break;
        default:
          LogSwitchStateWarning(this);
          break;
      }
    }

    public void InitialiseActionTrigger(float radius)
    {
      _actionRadius = radius;
    }

    #endregion

    #region Cooperation trigger

    private void RequestCooperation()
    {
      audioController.Play(AudioController.AudioType.Cooperate);
      EventManager.Instance.RequestCooperation(transform.position, currentTarget, _faction);
    }

    public void CooperateWithOthers(Transform target)
    {
      if (stateLock != NpcStateLock.Off) return;
      isCooperating = true;
      SetCurrentTarget(target);
      SwitchState(CooperateState);
      //Helper.Log("[NPC] " + transform.name + " is cooperating.");
    }

    #endregion

    #region Interaction trigger

    public void InteractWithObject(Transform target, bool started)
    {
      // Guard clause - Only take action if StateLock is Partial or Off
      if (stateLock == NpcStateLock.Full) return;

      if (transform.position == target.position)
      {
        if (started == false)
        {
          Helper.Log("[NPC] " + transform.name + " is no longer interacting with " +
                     currentTarget.tag + ".");
          RemoveCurrentTarget();
          return;
        }

        var colliders = Physics.OverlapSphere(
          transform.TransformPoint(_awarenessColliderCenter), _awarenessRadius, searchLayers);
        foreach (var collider in colliders)
          if (collider.CompareTag("Player"))
          {
            SetCurrentTarget(collider.transform);
            SwitchState(ObserveState);
          }

        // TODO: Consider extending this to NPCs so they can interact with each other, however
        // there's currently no use case for this.
        Helper.Log("[NPC] " + transform.name + " is interacting with " + currentTarget.tag + ".");
      }
    }

    #endregion

    #region Managing event

    private void OnEnable()
    {
      EventManager.Instance.OnNpcInteractionWithObject += InteractWithObject;
    }

    private void OnDestroy()
    {
      EventManager.Instance.OnNpcInteractionWithObject -= InteractWithObject;
    }

    #endregion
  }
}