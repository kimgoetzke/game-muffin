using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHindsight.Core;
using CaptainHindsight.Data.SFX;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace CaptainHindsight.Managers
{
  public class SfxManager : MonoBehaviour
  {
    private static SfxManager _instance;

    public static SfxManager Instance
    {
      get
      {
        if (_instance == null)
          _instance = FindObjectOfType<SfxManager>();

        return _instance;
      }
    }

    #region Defining variables

    [Title("Configuration")] [SerializeField]
    public AudioMixerGroup audioMixerGroup;
    
    [TitleGroup("SFX")]
    [TabGroup("SFX/SFX", "Effects")]
    [AssetList(Path = "/Data/SFX/Effects", AutoPopulate = true)]
    [ListDrawerSettings(ShowFoldout = true)]
    public List<SfxClip> effectSfx;
    
    [TabGroup("SFX/SFX", "Equipment")]
    [AssetList(Path = "/Data/SFX/Equipment", AutoPopulate = true)]
    [ListDrawerSettings(ShowFoldout = true)]
    public List<SfxClip> equipmentSfx;
    
    [TabGroup("SFX/SFX", "NPCs")]
    [HideLabel]
    [LabelText("NPC SFX")]
    [AssetList(Path = "/Data/SFX/NPCs", AutoPopulate = true)]
    [ListDrawerSettings(ShowFoldout = true)]
    public List<SfxClip> npcSfx;
    
    [TabGroup("SFX/SFX", "Player")]
    [AssetList(Path = "/Data/SFX/Player", AutoPopulate = true)]
    [ListDrawerSettings(ShowFoldout = true)]
    public List<SfxClip> playerSfx;
    
    [TitleGroup("SFX")]
    [TabGroup("SFX/SFX", "Ambience")]
    [AssetList(Path = "/Data/SFX/Ambience", AutoPopulate = true)]
    [ListDrawerSettings(ShowFoldout = true)]
    public List<SfxClip> ambienceSfx;

    [HorizontalGroup("SFX/AudioSource")] [SerializeField]
    private AudioSource defaultAudioSource;

    [HorizontalGroup("SFX/AudioSource")]
    [ShowIf("@defaultAudioSource == null")]
    [GUIColor(1f, 0.5f, 0.5f)]
    [Button]
    private void AddAudioSource()
    {
      defaultAudioSource = gameObject.GetComponent<AudioSource>();

      if (defaultAudioSource != null) return;
      defaultAudioSource = gameObject.AddComponent<AudioSource>();
      defaultAudioSource.outputAudioMixerGroup = audioMixerGroup;
    }

    #endregion

    private void Start()
    {
      ResetDefaultAudioSource(defaultAudioSource);
    }

    public static void PlaySfx(SfxClip sfx, AudioSource audioSource,
      bool waitToFinish = false)
    {
      if (audioSource.isPlaying && waitToFinish) return;
      audioSource.clip = sfx.clip;
      audioSource.volume = sfx.volume + Random.Range(-sfx.volumeVariation, sfx.volumeVariation);
      audioSource.pitch = sfx.pitch + Random.Range(-sfx.pitchVariation, sfx.pitchVariation);
      audioSource.Play();
    }
    
    public static async void PlaySfx(SfxClip sfx, bool waitToFinish = false)
    {
      var audioSource = _instance.defaultAudioSource;
      if (audioSource.isPlaying && waitToFinish) return;
      audioSource.clip = sfx.clip;
      audioSource.volume = sfx.volume + Random.Range(-sfx.volumeVariation, sfx.volumeVariation);
      audioSource.pitch = sfx.pitch + Random.Range(-sfx.pitchVariation, sfx.pitchVariation);
      audioSource.Play();
      await Task.Delay(TimeSpan.FromSeconds(sfx.clip.length));
      ResetDefaultAudioSource(audioSource);
    }
    
    private static void ResetDefaultAudioSource(AudioSource audioSource)
    {
      audioSource.clip = null;
      audioSource.volume = 1;
      audioSource.pitch = 1;
    }
  }
}