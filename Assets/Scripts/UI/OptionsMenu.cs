using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHindsight.Core;
using CaptainHindsight.Directors;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaptainHindsight.UI
{
  // This class implements BaseSubMenu which features the following methods:
  // - OpenMenu (abstract)
  // - BackToPreviousMenu (virtual)
  // - MoveButtonsOutOfView & MoveButtonsIntoView
  // - SetRaycaster
  // - OnEnable (which disables the raycaster)
  public class OptionsMenu : BaseSubMenu
  {
    [Title("General configuration")] [SerializeField, Required]
    public GameObject optionsMenuUI;

    [SerializeField, Required] private Slider musicVolumeSlider;
    [SerializeField, Required] private Slider sfxVolumeSlider;

    [SerializeField, Required] private TextMeshProUGUI musicVolumeText;
    [SerializeField, Required] private TextMeshProUGUI sfxVolumeText;

    [SerializeField, Required] private List<GameObject> options;
    [SerializeField, Required] private float inDuration = 0.3f;
    [SerializeField, Required] private float outDuration = 0.1f;

    [ShowInInspector, ReadOnly] private IMenuNestable _parentMenu;
    private AudioDirector _audioDirector;

    private void Awake()
    {
      sfxVolumeSlider.onValueChanged.AddListener(ActionSfxVolumeChanged);
      musicVolumeSlider.onValueChanged.AddListener(ActionMusicVolumeChanged);
      foreach (var element in options)
      {
        element.transform
          .DOScale(0, 0)
          .SetEase(Ease.Linear)
          .SetUpdate(UpdateType.Normal, true);
      }
    }

    private void Start() => _audioDirector = AudioDirector.Instance;

    #region Manage Options menu

    public override void OpenMenu(IMenuNestable origin)
    {
      LoadCurrentOptionValues();
      _parentMenu = origin;
      SetRaycaster(true);
      optionsMenuUI.SetActive(true);
      MoveButtonsIntoView();
      ShowOptions(inDuration);
    }

    private void LoadCurrentOptionValues()
    {
      sfxVolumeSlider.value = _audioDirector.GetVolume(GlobalConstants.AUDIO_SFX_MIXER);
      var volume = sfxVolumeSlider.value * 100 / sfxVolumeSlider.maxValue;
      sfxVolumeText.text = volume <= 1f ? "OFF" : volume.ToString("0");
      musicVolumeSlider.value = _audioDirector.GetVolume(GlobalConstants.AUDIO_MUSIC_MIXER);
      volume = musicVolumeSlider.value * 100 / musicVolumeSlider.maxValue;
      musicVolumeText.text = volume <= 1f ? "OFF" : volume.ToString("0");
    }

    private async void ShowOptions(float duration)
    {
      foreach (var element in options)
      {
        element.transform
          .DOScale(1f, duration)
          .SetEase(Ease.Linear)
          .SetUpdate(UpdateType.Normal, true);
        await Task.Delay(TimeSpan.FromSeconds(duration / options.Count));
      }

      await Task.Delay(TimeSpan.FromSeconds(duration));
    }

    public override async void BackToPreviousMenu()
    {
      base.BackToPreviousMenu();
      PlayButtonPressSound();
      await HideOptions(outDuration);
      _parentMenu.ReturnFromSubMenu();
      optionsMenuUI.SetActive(false);
    }

    private static void PlayButtonPressSound()
    {
      AudioDirector.Instance.Play("Click");
    }

    private async Task HideOptions(float duration)
    {
      _audioDirector.TrySavePlayerPrefs("_");

      foreach (var element in options)
      {
        element.transform
          .DOScale(new Vector3(0, 0, 0), duration)
          .SetEase(Ease.Linear)
          .SetUpdate(UpdateType.Normal, true);
        await Task.Delay(TimeSpan.FromSeconds(duration / options.Count));
      }

      await Task.Delay(TimeSpan.FromSeconds(duration));
    }

    #endregion

    #region Manage volume options

    private void ActionSfxVolumeChanged(float value)
    {
      ActionVolumeChange("SFX", sfxVolumeSlider, sfxVolumeText, value);
    }

    private void ActionMusicVolumeChanged(float value)
    {
      ActionVolumeChange("Music", musicVolumeSlider, musicVolumeText, value);
    }

    private void ActionVolumeChange(String label, Slider slider, TextMeshProUGUI mesh, float value)
    {
      _audioDirector.SetVolume(label, value);
      var volume = value * 100 / slider.maxValue;
      if (volume > 1) mesh.text = volume.ToString("0");
      if (volume.ToString("0") == "1") mesh.text = "OFF";
    }

    #endregion
  }
}