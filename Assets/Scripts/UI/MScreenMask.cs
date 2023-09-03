using CaptainHindsight.Managers;
using CaptainHindsight.Other;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace CaptainHindsight.UI
{
  public class MScreenMask : MonoBehaviour
  {
    [SerializeField, Required] private Image screenMask;
    [SerializeField] private float lengthOfFadeOut = 0.25f;
    [SerializeField] private float multiplier = 3.5f;
    private float _maxVisibility;
    private MCameraShake _cameraShake;
    [SerializeField] private float screenShakeIntensity = 1f;
    [SerializeField] private float screenShakeDuration = 0.2f;

    private void Awake()
    {
      EventManager.Instance.OnPlayerTakesDamage += TriggerMask;
      screenMask.gameObject.SetActive(false);
      _maxVisibility = screenMask.color.a;
      screenMask.color = new Color(screenMask.color.r, screenMask.color.g, screenMask.color.b, 0);
    }

    private void Start() => _cameraShake = MCameraShake.Instance;

    private void TriggerMask(int damage, int newHealth)
    {
      var visibility = Mathf.Clamp(((float)damage / newHealth) * multiplier, 0.1f, _maxVisibility);
      screenMask.gameObject.SetActive(true);
      _cameraShake.ShakeCamera(screenShakeIntensity, screenShakeDuration);
      var sequence = DOTween.Sequence();
      sequence.Append(screenMask.DOFade(visibility, 0));
      sequence.Append(screenMask.DOFade(0, lengthOfFadeOut));
      sequence.Play();
      sequence.onComplete += () => screenMask.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
      EventManager.Instance.OnPlayerTakesDamage -= TriggerMask;
    }
  }
}