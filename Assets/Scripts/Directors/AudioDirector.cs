using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHindsight.Core;
using CaptainHindsight.Managers;
using CaptainHindsight.Player;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

namespace CaptainHindsight.Directors
{
  public class AudioDirector : MonoBehaviour, IPlayerPrefsSaveable
  {
    public static AudioDirector Instance;

    #region Defining variables

    [Title("Configuration")] [SerializeField]
    private AudioMixer audioMixer;

    public AudioMixerGroup audioMixerGroup;
    private float[] _startVolume;
    private float _countdownBackgroundMusic;
    private float _lengthOfCurrentMusicTrack = 100f;

    [Title("Clips from Resources folder", "Automatically added but non-configurable")]
    [ShowInInspector]
    [ReadOnly]
    private AudioClip[] _soundClips;

    private Dictionary<string, AudioSource> _sourceList;

    [Title("Background music")] [SerializeField] [LabelText("Load music from:")]
    private GameObject backgroundMusicObject;

    [ShowInInspector] [ReadOnly] [ListDrawerSettings(ShowFoldout = false)]
    private AudioSource[] _backgroundMusic;

    [ShowInInspector, ReadOnly] private bool _isPlayingBackgroundMusic;

    #endregion

    #region Awake & initialisation

    private void Awake()
    {
      if (Instance == null)
      {
        Instance = this;
        DontDestroyOnLoad(gameObject);
      }
      else
      {
        Destroy(gameObject);
        return;
      }

      // Load all files of type audio clip from \Resources\Audio
      _soundClips = Resources.LoadAll<AudioClip>("Audio");

      // Create dictionary for audio sources which will be used later to find sounds (string) and
      // play the source (audio source)
      _sourceList = new Dictionary<string, AudioSource>();

      // Add audio source for each clip and add to sourceList
      foreach (var s in _soundClips)
      {
        var source = gameObject.AddComponent<AudioSource>();
        source.clip = s;
        source.outputAudioMixerGroup = audioMixerGroup; // All effects use the SFX mixer group
        _sourceList.Add(s.name, source);
      }

      // For trouble shooting sounds that don't play
      // foreach (KeyValuePair<string, AudioSource> kvp in sourceList) Helper.Log(kvp.Key + kvp.Value);
      // AudioSource a = sourceList["ButtonOff"];
      // a.Play();

      // Initialise and play background music
      SetAndStartBackgroundMusic();
    }

    // Load audio mixer settings (must be loaded at Start or later - known Unity problem)
    private void Start()
    {
      TryLoadPlayerPrefs();
    }

    private void SetAndStartBackgroundMusic()
    {
      // Get all audio sources from the background music object & play random track
      _backgroundMusic = backgroundMusicObject.GetComponents<AudioSource>();
      PlayBackgroundMusic();

      // Store starting volume so music can be turned down later
      _startVolume = new float[_backgroundMusic.Length];
      for (var i = 0; i < _backgroundMusic.Length; i++)
        _startVolume[i] = _backgroundMusic[i].volume;
    }

    public void PlayBackgroundMusic()
    {
      if (_isPlayingBackgroundMusic) return;
      _isPlayingBackgroundMusic = true;
      if (_backgroundMusic.Length == 0) return;
      var number = Random.Range(0, _backgroundMusic.Length);
      _backgroundMusic[number].Play();
      _lengthOfCurrentMusicTrack = _backgroundMusic[number].clip.length;
      // Helper.Log("[AudioDirector] Now playing track no. " + number + " (out of " + backgroundMusic.Length + "), length: " + backgroundMusic[number].clip.length + " seconds.");
    }

    public void StopBackgroundMusic()
    {
      _isPlayingBackgroundMusic = false;
      foreach (var audioSource in _backgroundMusic)
      {
        if (audioSource.isPlaying) audioSource.Stop();
      }
    }

    // TODO: Consider replacing this Update() loop by a coroutine/tasks or events
    private void Update()
    {
      _countdownBackgroundMusic += Time.deltaTime;
      if (!(_countdownBackgroundMusic >= _lengthOfCurrentMusicTrack)) return;
      _countdownBackgroundMusic = 0f;
      if (_isPlayingBackgroundMusic == false) return;
      PlayBackgroundMusic();
    }

    #endregion

    #region Managing methods for other scripts to call

    public void Play(string fileName)
    {
      if (_sourceList.ContainsKey(fileName) == false || _sourceList[fileName] == null)
      {
        Helper.LogWarning("[AudioDirector] Sound '" + fileName + "' not found.");
        return;
      }

      _sourceList[fileName].Play();
    }

    public void StopPlaying(string fileName)
    {
      if (_sourceList.ContainsKey(fileName) == false || _sourceList[fileName] == null)
      {
        Helper.LogWarning("[AudioDirector] Sound '" + fileName + "' not found.");
      }
      else if (_sourceList[fileName].isPlaying)
      {
        _sourceList[fileName].Stop();
      }
      // else Helper.Log("AudioManager: Tried to stop playing " + name + " but it's not playing.");
    }

    public void PlayWithoutOverlap(string fileName)
    {
      if (_sourceList.ContainsKey(fileName) == false || _sourceList[fileName] == null)
      {
        Helper.LogWarning("[AudioDirector] Sound '" + fileName + "' not found.");
        return;
      }

      if (!_sourceList[fileName].isPlaying) _sourceList[fileName].Play();
    }

    public AudioSource InitialiseSfx(GameObject obj, Sfx sfx, bool isStatic, int maxDistance = 15)
    {
      // This method adds an audio source to the provided game object and sets all its settings
      // based on the SFX object. It is used by other scene objects to initialise audio with
      // default settings. Set it like this:
      // audioSource = AudioDirector.Instance.AddAudioSourceForSfx(gameObject, sfx);
      // Then play it like this:
      // audioSource.Play();
      // This will add a new AudioSource for the SFX to the GameObject provided.

      var audioSource = AddAudioSource(obj, sfx);
      if (isStatic)
      {
        audioSource.spatialBlend = 0f;
        audioSource.maxDistance = 200f;
      }
      else
      {
        audioSource.spatialBlend = 1f;
        audioSource.maxDistance = maxDistance;
      }

      audioSource.loop = true;
      return audioSource;
    }

    public AudioSource InitialiseSfx(GameObject obj, Sfx sfx)
    {
      // This method adds an audio source to the provided game object and sets all its settings
      // based on the SFX object. It is used by other scene objects to initialise audio with
      // default settings. Set it like this:
      // audioSource = AudioDirector.Instance.AddAudioSourceForSfx(gameObject, sfx);
      // Then play it like this:
      // audioSource.Play();
      // This will add a new AudioSource for the SFX to the GameObject provided.

      var audioSource = AddAudioSource(obj, sfx);
      audioSource.spatialBlend = 1f;
      audioSource.maxDistance = 15f;
      return audioSource;
    }

    private AudioSource AddAudioSource(GameObject obj, Sfx sfx)
    {
      var audioSource = obj.AddComponent<AudioSource>();
      audioSource.clip = sfx.sfxClip.clip;
      audioSource.volume = sfx.sfxClip.volume;
      audioSource.pitch = sfx.sfxClip.pitch;
      audioSource.rolloffMode = AudioRolloffMode.Linear;
      audioSource.dopplerLevel = 0f;
      audioSource.playOnAwake = false;
      audioSource.outputAudioMixerGroup = audioMixerGroup;

      return audioSource;
    }

    #endregion

    #region Manage background music & ambient sounds

    public void ReduceBackgroundMusicVolume()
    {
      Helper.Log("[AudioDirector] Background music volume is being reduced.");
      foreach (var track in _backgroundMusic)
        track.DOFade(0.15f, 1f);
    }

    public async void IncreaseBackgroundMusicVolume()
    {
      await Task.Delay(System.TimeSpan.FromSeconds(1f));
      Helper.Log("[AudioDirector] Background music volume is being increased again.");
      for (var i = 0; i < _backgroundMusic.Length; i++)
        _backgroundMusic[i].DOFade(_startVolume[i], 2f);
    }

    #endregion

    public void SetVolume(string mixer, float volume)
    {
      audioMixer.SetFloat(mixer, Mathf.Log10(volume) * 30f);
    }

    public float GetVolume(string mixer)
    {
      audioMixer.GetFloat(mixer, out var volume);
      return Mathf.Pow(10, volume / 30f);
    }

    #region Get player audio settings from PlayerPrefs

    public void TryLoadPlayerPrefs()
    {
      Helper.Log("[AudioDirector] Loading audio settings.");
      var sfx = PlayerPrefsManager.Instance.LoadFloat(GlobalConstants.AUDIO_SFX_VOLUME);
      if (sfx > 0) audioMixer.SetFloat("SFX", ToLogScale(sfx));
      var music = PlayerPrefsManager.Instance.LoadFloat(GlobalConstants.AUDIO_MUSIC_VOLUME);
      if (music > 0) audioMixer.SetFloat("Music", ToLogScale(music));
      DebugVolumeSettings(sfx, music);
    }

    public void TrySavePlayerPrefs(string ignored)
    {
      Helper.Log("[AudioDirector] Saving audio settings.");
      audioMixer.GetFloat("SFX", out var sfx);
      sfx = ToLinearScale(sfx);
      PlayerPrefsManager.Instance.Save(GlobalConstants.AUDIO_SFX_VOLUME, sfx);
      audioMixer.GetFloat("Music", out var music);
      music = ToLinearScale(music);
      PlayerPrefsManager.Instance.Save(GlobalConstants.AUDIO_MUSIC_VOLUME, music);
      DebugVolumeSettings(sfx, music);
    }
    
    private static float ToLogScale(float value)
    {
      return Mathf.Log10(value) * 30f;
    }
    
    private static float ToLinearScale(float value)
    {
      return Mathf.Pow(10, value / 30f);
    }

    private void DebugVolumeSettings(float sfx, float music)
    {
      // For debugging:
      Helper.Log($"[AudioDirector] Current volume settings - SFX: {sfx}. Music: {music}.");
    }

    #endregion
  }
}