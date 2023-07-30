using System;
using System.Threading;
using System.Threading.Tasks;
using CaptainHindsight.Core;
using CaptainHindsight.Core.Queue;
using CaptainHindsight.Directors;
using CaptainHindsight.Managers;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaptainHindsight.UI
{
  public class BannerBar : MonoBehaviour
  {
    [SerializeField] [Required] private GameObject bannerBarHolder;

    [SerializeField] [Required] private Image background;

    private const float BAlpha = 0.6f;

    [SerializeField] [Required] private TextMeshProUGUI titleMesh;

    [SerializeField] [Required] private GameObject titleGameObject;

    [SerializeField] [Required] private GameObject horizontalBarGameObject;

    [SerializeField] [Required] private TextMeshProUGUI messageMesh;

    [SerializeField] [Required] private GameObject messageGameObject;

    [ShowInInspector] private UniqueQueue<BannerBarMessage> _messageQueue = new();

    private bool _isProcessingQueue;
    private CancellationTokenSource _cancellationTokenSource;

    private void Awake()
    {
      // This method must subscribe for the relevant events in the Awake() method and unsubscribe
      // OnDestroy or subscribe in the OnEnable() method and unsubscribe in the OnDisable() method.
      EventManager.Instance.OnDisplayBannerBar += EnqueueMessage;
      EventManager.Instance.OnDisplayCountdown += EnqueueCountdown;
    }

    #region Manging and actioning queue

    private async void ProcessQueue()
    {
      if (_isProcessingQueue) return;
      _isProcessingQueue = true;
      while (_messageQueue.IsEmpty() == false)
      {
        var message = _messageQueue.Dequeue();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;
        if (message.MessageType == MessageType.Message)
          await DisplayMessage(message.Title, message.Message, message.Seconds,
            message.AudioClipName, cancellationToken);
        else if (message.MessageType == MessageType.Countdown)
          await DisplayCountdown(message.Seconds, cancellationToken);
      }

      _isProcessingQueue = false;
    }

    private async Task DisplayMessage(string title, string message, int seconds,
      string audioClipName, CancellationToken cancellationToken)
    {
      try
      {
        // For debugging:
        // Helper.Log("[BannerBar] Banner triggered (audioClipName=" + audioClipName +
        //            ") with '" + title + "' and message '" + message + "'.");

        // Initialise banner bar
        ShowTextElements();
        ShowBackground();
        var showMessage = false;
        var showTitle = false;
        var animationLength = 0.5f;
        AudioDirector.Instance.Play(audioClipName);

        // Set title if provided or disable layout element
        if (title.Length >= 1) showTitle = true;
        titleGameObject.SetActive(showTitle);
        SetTitle(title);

        // Set message if provided or disable layout element
        if (message.Length >= 1) showMessage = true;
        messageGameObject.SetActive(showMessage);
        SetMessage(message);

        // Set separating horizontal bar, if required
        if (showMessage & showTitle)
        {
          horizontalBarGameObject.SetActive(true);
        }
        else
        {
          horizontalBarGameObject.SetActive(false);
        }

        // Fade in
        bannerBarHolder.SetActive(true);
        background
          .DOFade(BAlpha, animationLength)
          .SetUpdate(UpdateType.Normal, true)
          .OnComplete(() => { titleMesh.gameObject.SetActive(true); });

        // Wait while banner is being displayed
        await Task.Delay(TimeSpan.FromSeconds(seconds), cancellationToken);

        // Fade out
        ShowTextElements(false);
        background
          .DOFade(0f, animationLength)
          .SetUpdate(UpdateType.Normal, true);
        await Task.Delay(TimeSpan.FromSeconds(animationLength), cancellationToken);
        bannerBarHolder.SetActive(false);
      }
      catch (TaskCanceledException)
      {
        Helper.Log("[MBannerBar] Banner cancelled.");
      }
    }

    private async Task DisplayCountdown(int seconds, CancellationToken cancellationToken)
    {
      try
      {
        // For debugging:
        Helper.Log("[MBannerBar] Countdown triggered, counting down from " + seconds + "...");

        // Initialise banner bar
        ShowBackground(false);
        titleGameObject.SetActive(false);
        messageGameObject.SetActive(true);
        horizontalBarGameObject.SetActive(false);
        var fontSize = messageMesh.fontSize;
        messageMesh.fontSize = 70;

        // Counting down, then reset font size to normal
        for (var i = seconds; i >= 0; i--)
        {
          bannerBarHolder.SetActive(true);
          if (i > 0)
          {
            SetMessage("- " + i.ToString() + " -");
            AudioDirector.Instance.Play("Tick");
          }
          else
          {
            SetMessage("- GO! -");
            AudioDirector.Instance.Play("Boom");
          }

          messageMesh.DOFade(0.05f, 1f);
          var elapsedTime = 0f;
          while (elapsedTime < 1f)
          {
            elapsedTime += Time.deltaTime;
            await Task.Yield();
          }

          bannerBarHolder.SetActive(false);
          messageMesh.DOFade(1f, 0f);
        }

        messageMesh.fontSize = fontSize;
        await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
      }
      catch (TaskCanceledException)
      {
        Helper.Log("[MBannerBar] Countdown cancelled.");
      }
    }

    private void SetTitle(string title)
    {
      titleMesh.text = title;
    }

    private void SetMessage(string message)
    {
      messageMesh.text = message;
    }

    private void ShowBackground(bool status = true)
    {
      background.enabled = status;
    }

    private void ShowTextElements(bool status = true)
    {
      messageMesh.color =
        new Color(messageMesh.color.r, messageMesh.color.g, messageMesh.color.b, 1f);
      titleGameObject.SetActive(status);
      messageGameObject.SetActive(status);
      horizontalBarGameObject.SetActive(status);
    }

    #endregion

    #region Managing events

    private void EnqueueCountdown(int seconds)
    {
      if (GameStateDirector.Instance.CurrentState() == GameState.Transition) return;
      _messageQueue.Enqueue(new BannerBarMessage(seconds));
      ProcessQueue();
    }

    private void EnqueueMessage(string title, string message, int seconds,
      string audioClipName = "Bing")
    {
      if (GameStateDirector.Instance.CurrentState() == GameState.Transition) return;
      _messageQueue.Enqueue(new BannerBarMessage(title, message, seconds, audioClipName ?? "Bing"));
      ProcessQueue();
    }

    private void OnDestroy()
    {
      _messageQueue.Clear();
      _cancellationTokenSource?.Cancel();
      _isProcessingQueue = false;
      EventManager.Instance.OnDisplayBannerBar -= EnqueueMessage;
      EventManager.Instance.OnDisplayCountdown -= EnqueueCountdown;
    }

    #endregion

    #region Managing classes

    [Serializable]
    private class
      BannerBarMessage
    {
      public readonly MessageType MessageType;
      public readonly string Title;
      public readonly string Message;
      public readonly int Seconds;
      public readonly string AudioClipName;

      public BannerBarMessage(string title, string message, int seconds, string audioClipName)
      {
        MessageType = MessageType.Message;
        Title = title;
        Message = message;
        Seconds = seconds;
        AudioClipName = audioClipName;
      }

      public BannerBarMessage(int seconds)
      {
        MessageType = MessageType.Countdown;
        Title = "";
        Message = "";
        Seconds = seconds;
        AudioClipName = null;
      }

      public override string ToString()
      {
        return "{" + MessageType + " with title='" + Title + "', message='" + Message +
               "', seconds=" + Seconds + ", audioClipName=" + AudioClipName + "}";
      }

      public override bool Equals(object obj)
      {
        if (obj is BannerBarMessage other)
          return Title == other.Title && Message == other.Message && Seconds == other.Seconds &&
                 AudioClipName == other.AudioClipName;

        return false;
      }

      public override int GetHashCode()
      {
        return (MessageType: MessageType, Title: Title, Message: Message, Seconds: Seconds,
          AudioClipName: AudioClipName).GetHashCode();
      }
    }

    #endregion
  }
}