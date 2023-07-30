using System;
using System.Threading;
using System.Threading.Tasks;
using CaptainHindsight.Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;

namespace CaptainHindsight.Directors.Audio
{
  public class AudioAmbience : MonoBehaviour
  {
    [SerializeField, Required] private Sfx sfx;
    [SerializeField] private bool isSpatialSound;

    [SerializeField] [ShowIf("isSpatialSound")]
    private int maxDistance = 20;

    private AudioSource _audioSource;
    private CancellationTokenSource _cancellationTokenSource;
    private float _maxVolume = 1f;

    private void Awake()
    {
      _audioSource =
        AudioDirector.Instance.InitialiseSfx(gameObject, sfx, !isSpatialSound, maxDistance);
      _maxVolume = sfx.sfxClip.volume;
    }

    public async void Play()
    {
      _cancellationTokenSource?.Cancel();
      _cancellationTokenSource = new CancellationTokenSource();
      _audioSource.Play();
      var cancellationToken = _cancellationTokenSource.Token;
      await FadeAudio(0f, _maxVolume, 3f, cancellationToken);
    }

    public async void Stop()
    {
      _cancellationTokenSource?.Cancel();
      _cancellationTokenSource = new CancellationTokenSource();
      var cancellationToken = _cancellationTokenSource.Token;
      await FadeAudio(-1f, 0f, 3f, cancellationToken);
      _audioSource.Stop();
    }

    private async Task FadeAudio(float start, float end, float duration,
      CancellationToken cancellationTokenSource)
    {
      try
      {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (start != -1f) _audioSource.volume = start;
        _audioSource.DOFade(end, duration);
        await Task.Delay(TimeSpan.FromSeconds(duration), cancellationTokenSource);
      }
      catch
      {
        // Helper.Log("[AudioAmbience] Fade cancelled.");
      }
    }

    private void OnDestroy()
    {
      _cancellationTokenSource?.Cancel();
    }
  }
}