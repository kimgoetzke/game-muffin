using CaptainHindsight.Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CaptainHindsight.Directors.Audio
{
  public class AudioOneShot : MonoBehaviour
  {
    [SerializeField, Required] private Sfx sfx;

    [FormerlySerializedAs("isPlayingOnStart")] [SerializeField]
    private bool isPlayingOnEnable = true;

    [ShowInInspector, ReadOnly] private AudioSource _audioSource;

    // To use this, simply add this script to gameObject from which you want to call it,
    // then use in the code with:
    // private AudioOneShot _audioOneShot;
    // _audioOneShot = GetComponent<AudioOneShot>();
    // If you don't want isPlayingOnEnable, you can just play the sound with:
    // _audioOneShot.Play();

    private void Awake()
    {
      _audioSource =
        AudioDirector.Instance.InitialiseSfx(gameObject, sfx);
    }

    private void OnEnable()
    {
      if (!isPlayingOnEnable) return;
      _audioSource.Play();
    }

    public void Play()
    {
      _audioSource.Play();
      RandomisePitchAndVolume();
    }

    private void RandomisePitchAndVolume()
    {
      _audioSource.volume = sfx.sfxClip.volume +
                            Random.Range(-sfx.sfxClip.volumeVariation,
                              sfx.sfxClip.volumeVariation);
      _audioSource.pitch =
        sfx.sfxClip.pitch + Random.Range(-sfx.sfxClip.pitchVariation,
          sfx.sfxClip.pitchVariation);
    }
  }
}