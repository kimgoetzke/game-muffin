using CaptainHindsight.Core;
using CaptainHindsight.Directors.Audio;
using CaptainHindsight.Managers;
using CaptainHindsight.NPCs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.UI
{
  public class AreaZoneTrigger : MonoBehaviour
  {
    [Title("Banner Bar")] [SerializeField] private bool hasBannerBar = true;

    [SerializeField] [ShowIf("hasBannerBar")]
    private string title;

#pragma warning disable 0414
    [SerializeField] [ShowIf("hasBannerBar")]
    private bool showMessage = false;
#pragma warning restore 0414

    [SerializeField] [ShowIf("hasBannerBar")] [EnableIf("showMessage")]
    private string message;

    [SerializeField] [ShowIf("hasBannerBar")] [MinValue(1)]
    private int bannerDuration = 2;

    [Title("Linked Settings")]
    [InfoBox(
      "If an event is linked to this component, the component's OnTriggerEnter event will be disabled as the banner bar will be updated and triggered manually by the event.")]
    [ShowInInspector]
    [ReadOnly]
    [ShowIf("showMessage")]
    private bool _linkedEvent;

    [ShowInInspector] [ReadOnly] [ShowIf("_linkedEvent")]
    private bool _eventInProgress;

    [ShowInInspector] [ReadOnly] [ShowIf("_linkedEvent")]
    private EnemyWaveController _waveController;

    [Title("Audio")] [SerializeField] private bool hasAudio;

    [ShowInInspector, ReadOnly] [ShowIf("hasAudio")]
    private AudioAmbience _audioAmbience;

    private void Awake()
    {
      if (hasAudio)
      {
        _audioAmbience = GetComponent<AudioAmbience>();
      }
    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.CompareTag("Player") == false) return;
      if (hasAudio) _audioAmbience.Play();
      if (hasBannerBar) TriggerBannerBar();
      if (_linkedEvent && _eventInProgress && _waveController != null)
        _waveController.PauseEvent(false);
    }

    private void OnTriggerExit(Collider other)
    {
      if (other.CompareTag("Player") == false) return;
      if (hasAudio) _audioAmbience.Stop();
      if (_linkedEvent && _eventInProgress && _waveController != null)
      {
        EventManager.Instance.DisplayBannerBar(title, "Event paused", bannerDuration);
        _waveController.PauseEvent();
      }
    }

    public void TriggerBannerBar(string audioClipName = null)
    {
      EventManager.Instance.DisplayBannerBar(title, message, bannerDuration, audioClipName);
      // For debugging:
      // Helper.Log("[AreaZoneTrigger] Triggered message: '" + title + "' / '" + message + "'.");
    }

    public void TriggerCountdown(int seconds)
    {
      EventManager.Instance.DisplayCountdown(seconds);
      // For debugging:
      // Helper.Log("[AreaZoneTrigger] Triggered countdown.");
    }

    public void Configure(bool status, bool setController = false,
      EnemyWaveController component = null)
    {
      _linkedEvent = true;
      _eventInProgress = !status;
      if (setController) _waveController = component;
    }

    public void UpdateMessageText(string text)
    {
      message = text;
    }

    public void PrependMessageText(string text)
    {
      message = text + message;
    }

    public void UpdateTitleText(string text)
    {
      title = text;
    }

    public string GetTitle()
    {
      return title;
    }
  }
}