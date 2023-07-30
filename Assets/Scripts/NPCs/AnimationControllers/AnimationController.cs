using CaptainHindsight.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CaptainHindsight.NPCs.AnimationControllers
{
  public class AnimationController : MonoBehaviour
  {
    [FormerlySerializedAs("Animator")] [Title("Animation Controller")]
    public Animator animator;

    [FormerlySerializedAs("IsFacingRight")] [ShowInInspector] [ReadOnly]
    public bool isFacingRight;

    public virtual void UpdateAnimationAndRotation(float x, float y)
    {
    }

    public virtual void FaceTarget(Transform target)
    {
    }

    public virtual void ChangeAnimationLayer(int layer, float weight, bool reset = false)
    {
    }

    public virtual NpcType GetNpcType()
    {
      return NpcType.Unspecified;
    }

    public virtual void SetTrigger(string animTrigger, bool randomInt = false,
      NpcAnimationTrigger trigger = NpcAnimationTrigger.Unspecified)
    {
      animator.SetTrigger(animTrigger);
    }

    public void SetBool(string name, bool status, bool single = false)
    {
      if (single) ResetBools();

      animator.SetBool(name, status);
    }

    public void ResetBools()
    {
      foreach (var parameter in animator.parameters)
        if (parameter.type == AnimatorControllerParameterType.Bool)
          animator.SetBool(parameter.name, false);
    }

    public void SetFloat(string name, float value)
    {
      animator.SetFloat(name, value);
    }

    public void SetInteger(string name, int value)
    {
      animator.SetInteger(name, value);
    }

    public void SetAnimatorSpeed(float speed)
    {
      animator.speed = speed;
    }

    public bool GetBool(string name)
    {
      return animator.GetBool(name);
    }

    public float GetCurrentAnimatorStateInfo(int layer)
    {
      return animator.GetCurrentAnimatorStateInfo(layer).length;
    }
  }
}