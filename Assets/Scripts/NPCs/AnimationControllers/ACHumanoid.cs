using CaptainHindsight.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CaptainHindsight.NPCs.AnimationControllers
{
  public class AcHumanoid : AnimationController
  {
    [FormerlySerializedAs("Type")] [Title("Humanoid")] [ReadOnly]
    public NpcType type = NpcType.Humanoid;

    [SerializeField] private Transform[] skeletons;
    private const int AttackAnimations = 2;
    private Transform _currentSkeleton;
    private int _currentDirection;
    private static readonly int MoveSpeed = Animator.StringToHash("moveSpeed");

    private void Awake()
    {
      // Make sure only skeleton for currentDirection is active
      foreach (var skeleton in skeletons) skeleton.gameObject.SetActive(false);
      _currentSkeleton = skeletons[_currentDirection];
      _currentSkeleton.gameObject.SetActive(true);
    }

    public override NpcType GetNpcType()
    {
      return type;
    }

    public override void UpdateAnimationAndRotation(float x, float y)
    {
      if (DirectionHasChanged(x)) UpdateSkeleton();
      animator.SetFloat(MoveSpeed, Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)));
    }

    public override void FaceTarget(Transform target)
    {
      var x = target.position.x - transform.position.x;
      if (DirectionHasChanged(x)) UpdateSkeleton();
    }

    public override void ChangeAnimationLayer(int layer, float weight, bool reset = false)
    {
      if (reset)
      {
        ResetLayers();
        return;
      }

      animator.SetLayerWeight(layer, weight);
    }

    private void ResetLayers()
    {
      for (var i = 0; i < animator.layerCount; i++) animator.SetLayerWeight(i, 0);

      animator.SetLayerWeight(0, 1);
      Helper.Log("[ACHumanoid] " + animator.layerCount + " animation layers were reset.");
    }

    public override void SetTrigger(string animTrigger, bool randomInt = false,
      NpcAnimationTrigger trigger = NpcAnimationTrigger.Unspecified)
    {
      if (randomInt)
      {
        string intName;
        int value;
        switch (trigger)
        {
          case NpcAnimationTrigger.Attack:
            intName = "attackSequence";
            value = Random.Range(0, AttackAnimations);
            break;
          case NpcAnimationTrigger.Unspecified:
          default:
            Helper.LogWarning(
              "[ACHumanoid] Invalid SetTrigger command used. Random SetTrigger event requested but NPCAnimationTrigger does not exist.");
            return;
        }

        animator.SetInteger(intName, value);
      }

      base.SetTrigger(animTrigger);
    }

    private bool DirectionHasChanged(float x)
    {
      switch (x)
      {
        case < 0 when isFacingRight:
          _currentDirection = 0;
          isFacingRight = !isFacingRight;
          return true;
        case > 0 when isFacingRight == false:
          _currentDirection = 1;
          isFacingRight = !isFacingRight;
          return true;
        default:
          return false;
      }
    }

    private void UpdateSkeleton()
    {
      // Get layer weights from current skeleton
      var layers = new float[animator.layerCount];
      for (var i = 0; i < animator.layerCount; i++) layers[i] = animator.GetLayerWeight(i);
      // Helper.Log("Float " + i + " set to " + layers[i]);
      // Activate only skeleton for current direction
      _currentSkeleton.gameObject.SetActive(false);
      skeletons[_currentDirection].gameObject.SetActive(true);
      _currentSkeleton = skeletons[_currentDirection];

      // Update animator and animation
      animator = skeletons[_currentDirection].GetComponent<Animator>();
      animator.enabled = true;

      // Set layer weights for new skeleton
      for (var i = 0; i < animator.layerCount; i++)
      {
        animator.Play(_currentDirection == 0 ? "moveW" : "moveE", i);
        animator.SetLayerWeight(i, layers[i]);
        // Helper.Log("Layer " + i + " updates to " + layers[i]);
      }
    }
  }
}