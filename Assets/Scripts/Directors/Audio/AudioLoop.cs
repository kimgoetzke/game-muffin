using System.Threading;
using System.Threading.Tasks;
using CaptainHindsight.Managers;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Directors.Audio
{
  public class AudioLoop : MonoBehaviour
  {
    [SerializeField] private bool isSetManually;
    [SerializeField, Required] private Sfx sfx;

    [SerializeField] private int maxDistance = 6;
    private AudioSource _audioSource;
    private float _volume;
    private CancellationTokenSource _cancellationTokenSource;

    private void Awake()
    {
      _audioSource =
        AudioDirector.Instance.InitialiseSfx(gameObject, sfx, false, maxDistance);
      _audioSource.loop = true;
      _audioSource.playOnAwake = !isSetManually;
      _volume = sfx.sfxClip.volume;
    }

    private void Start()
    {
      if (isSetManually == false)
      {
        _audioSource.Play();
      }
    }

    public void SetPlay(bool isPlaying, bool isFading = false, float fadeTime = 1f)
    {
      if (isSetManually == false) return;
      DOTween.Kill(_audioSource);
      _cancellationTokenSource?.Cancel();
      _cancellationTokenSource = new CancellationTokenSource();
      ActionIsPlayingTask(isPlaying, isFading, fadeTime);
    }

    private async void ActionIsPlayingTask(bool isPlaying, bool isFading = false,
      float fadeTime = 1f)
    {
      try
      {
        if (isPlaying)
        {
          _audioSource.volume = _volume;
          _audioSource.Play();
          if (isFading == false) return;
          _audioSource.volume = 0;
          _audioSource.DOFade(_volume, fadeTime);
          await Task.Delay(System.TimeSpan.FromSeconds(fadeTime), _cancellationTokenSource.Token);
        }
        else
        {
          if (isFading)
          {
            _audioSource.DOFade(0, fadeTime);
            await Task.Delay(System.TimeSpan.FromSeconds(fadeTime), _cancellationTokenSource.Token);
          }

          _audioSource.Stop();
          _audioSource.volume = _volume;
        }
      }
      catch (TaskCanceledException)
      {
      }
    }

    public void FadeVolume(float delay, float from = -1, float to = 0,
      float over = 1f)
    {
      if (isSetManually == false) return;
      DOTween.Kill(_audioSource);
      _cancellationTokenSource?.Cancel();
      _cancellationTokenSource = new CancellationTokenSource();
      ActionFadeVolumeTask(delay, from, to, over);
    }

    private async void ActionFadeVolumeTask(float delay, float from, float to, float over)
    {
      try
      {
        await Task.Delay(System.TimeSpan.FromSeconds(delay), _cancellationTokenSource.Token);
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        _audioSource.volume = from == -1 ? _volume : from;
        _audioSource.DOFade(to, over);
        await Task.Delay(System.TimeSpan.FromSeconds(over), _cancellationTokenSource.Token);
        if (to == 0) _audioSource.Stop();
        _audioSource.volume = _volume;
      }
      catch (TaskCanceledException)
      {
      }
    }

    private void OnDestroy()
    {
      _cancellationTokenSource?.Cancel();
    }
  }
}