using CaptainHindsight.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CaptainHindsight.NPCs.AnimationControllers
{
  public class AcDinosaurs : AnimationController
  {
    [FormerlySerializedAs("Type")] [Title("Dinosaurs")] [ReadOnly]
    public NpcType type = NpcType.Dinosaurs;

    [SerializeField] [Required] private Transform awarenessTransform;
    [SerializeField] [Required] private Transform perspectiveTransform;
    private static readonly int IsRunning = Animator.StringToHash("isRunning");

    public override NpcType GetNpcType()
    {
      return type;
    }

    public override void UpdateAnimationAndRotation(float x, float y)
    {
      if (x < -0.1f)
      {
        animator.SetBool(IsRunning, true);
        if (isFacingRight) FlipLocalScale();
      }
      else if (x > 0.1f)
      {
        animator.SetBool(IsRunning, true);
        if (isFacingRight == false) FlipLocalScale();
      }
      else
      {
        animator.SetBool(IsRunning, false);
      }
    }

    public override void FaceTarget(Transform target)
    {
      if (target == null) return;
      var x = target.position.x - transform.position.x;
      if (x < 0 && isFacingRight) FlipLocalScale();
      else if (x > 0 && isFacingRight == false) FlipLocalScale();
    }

    private void FlipLocalScale()
    {
      if (isFacingRight)
      {
        perspectiveTransform.localScale = new Vector3(1f, 1f, 1f);
        awarenessTransform.localScale = new Vector3(1f, 1f, 1f);
      }
      else
      {
        perspectiveTransform.localScale = new Vector3(-1f, 1f, 1f);
        awarenessTransform.localScale = new Vector3(-1f, 1f, 1f);
      }

      isFacingRight = !isFacingRight;
    }
  }
}