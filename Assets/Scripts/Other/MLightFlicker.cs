using UnityEngine;

namespace CaptainHindsight.Other
{
  public class MLightFlicker : MonoBehaviour
  {
    [SerializeField] private Animator animator;
    [SerializeField] private Intensity intensity;

    private enum Intensity
    {
      Regular,
      Strong,
      Intense,
      Random
    };

    private void Start()
    {
      if (intensity == Intensity.Random)
        intensity = (Intensity)Random.Range(0f, (float)Intensity.Random);

      animator.SetTrigger(intensity.ToString());
    }
  }
}