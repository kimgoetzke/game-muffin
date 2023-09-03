using Cinemachine;
using UnityEngine;

namespace CaptainHindsight.Other
{
  public class MCameraShake : MonoBehaviour
  {
    public static MCameraShake Instance { get; private set; }
    private CinemachineVirtualCamera _cinemachineCamera;
    private float _shakeTimer;
    private float _shakeTimerTotal;
    private float _startingIntensity;

    private void Awake()
    {
      if (Instance == null)
      {
        Instance = this;
      }
      else
      {
        Destroy(gameObject);
        return;
      }

      _cinemachineCamera = GetComponent<CinemachineVirtualCamera>();
    }

    public void ShakeCamera(float intensity, float time)
    {
      var cinemachineBasicMultiChannelPerlin =
        _cinemachineCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        
      cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;
      _startingIntensity = intensity;
      _shakeTimerTotal = time;
      _shakeTimer = time;
    }

    private void Update()
    {
      if (_shakeTimer > 0)
      {
        _shakeTimer -= Time.deltaTime;
        var cinemachineBasicMultiChannelPerlin =
          _cinemachineCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        // Stop camera shake slowly over time
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = Mathf.Lerp(_startingIntensity, 0f,
          1 - _shakeTimer / _shakeTimerTotal);
      }
    }
  }
}