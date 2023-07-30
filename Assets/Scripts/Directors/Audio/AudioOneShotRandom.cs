using System.Collections.Generic;
using CaptainHindsight.Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CaptainHindsight.Directors.Audio
{
  public class AudioOneShotRandom : MonoBehaviour
  {
    [SerializeField, Required] private Sfx[] sfx;

    [FormerlySerializedAs("isPlayingOnEnable")] [SerializeField]
    private bool isPlayingOnStart = true;

    private bool _hasPlayed = true;

    [ShowInInspector, ReadOnly] private List<AudioSource> _audio = new();


    // To use this, simply add this script to gameObject from which you want to call it,
    // then use in the code with:
    // private AudioOneShot _audioOneShot;
    // _audioOneShotRandom = GetComponent<AudioOneShotRandom>();
    // If you don't want isPlayingOnEnable, you can just play the sound with:
    // _audioOneShotRandom.Play();

    private void Awake()
    {
      foreach (var s in sfx)
      {
        _audio.Add(
          AudioDirector.Instance.InitialiseSfx(gameObject, s));
      }
    }

    private void OnEnable()
    {
      if (!isPlayingOnStart || _hasPlayed) return;
      // Helper.Log("[MAudioOneShot] Playing " + sfx.sfxToPlay.Label + " on enable.");
      _audio[Random.Range(0, sfx.Length)].Play();
    }

    public void Play()
    {
      var index = Random.Range(0, sfx.Length);
      _audio[index].Play();
      RandomisePitchAndVolume(index);
    }

    private void RandomisePitchAndVolume(int index)
    {
      _audio[index].volume = sfx[index].sfxClip.volume + Random.Range(
        -sfx[index].sfxClip.volumeVariation, sfx[index].sfxClip.volumeVariation);
      _audio[index].pitch = sfx[index].sfxClip.pitch + Random.Range(
        -sfx[index].sfxClip.pitchVariation, sfx[index].sfxClip.pitchVariation);
    }

    private void OnDisable() => _hasPlayed = false;
  }
}