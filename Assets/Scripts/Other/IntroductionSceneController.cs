using System;
using System.Threading;
using CaptainHindsight.Core;
using CaptainHindsight.Directors;
using CaptainHindsight.Directors.Audio;
using CaptainHindsight.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace CaptainHindsight.Other
{
  public class IntroductionSceneController : MonoBehaviour
  {
    [SerializeField] private Transform holder;
    [SerializeField] private int textScrollDuration = 35;
    [SerializeField] private Transform spaceship;
    [SerializeField] private int flyDuration = 17;
    [SerializeField] private AudioOneShot audioOneShot;
    [SerializeField] private TextMeshProUGUI[] paragraphs;
    [SerializeField] private bool isChangingScene = true;
    [SerializeField] private string sceneName;
    private CancellationTokenSource _tokenSource1;
    private CancellationTokenSource _tokenSource2;

    private async void Awake()
    {
      DOTween.SetTweensCapacity(1250, 50);
      foreach (var paragraph in paragraphs)
      {
        paragraph.DOFade(0, 0);
      }

      holder.DOMoveY(-300, 0);
      holder.DOMoveY(900, textScrollDuration);
      spaceship.DOMove(new Vector3(0, 2.1f, -0.3f), flyDuration);
      _tokenSource1 = new CancellationTokenSource();
      FadeToNextScene();
      foreach (var paragraph in paragraphs)
      {
        var sequence = AnimateParagraph(paragraph);
        _tokenSource2 = new CancellationTokenSource();
        await PlayAudio(sequence);
        await WaitOneSecond();
      }
    }

    private static Sequence AnimateParagraph(TextMeshProUGUI paragraph)
    {
      try
      {
        var animator = new DOTweenTMPAnimator(paragraph);
        var sequence = DOTween.Sequence();
        sequence.Append(paragraph.DOFade(0, 0));
        for (var j = 0; j < animator.textInfo.characterCount; ++j)
        {
          if (!animator.textInfo.characterInfo[j].isVisible) continue;
          sequence.Append(animator.DOFadeChar(j, 1, 0.01f));
          sequence.Join(animator.DOPunchCharScale(j, new Vector3(1.1f, 1.1f, 1.1f), 0.02f));
        }

        return sequence;
      }
      catch (Exception e)
      {
        Helper.Log("[IntroductionScene] Cancelled.");
        return null;
      }
    }

    private async Task WaitOneSecond()
    {
      try
      {
        await Task.Delay(TimeSpan.FromSeconds(1f), _tokenSource2.Token);
      }
      catch (Exception)
      {
        Helper.Log("[IntroductionScene] Cancelled.");
      }
    }

    private async void FadeToNextScene()
    {
      try
      {
        await Task.Delay(TimeSpan.FromSeconds(flyDuration - 0.5f), _tokenSource1.Token);
        if (isChangingScene == false) return;
        
        audioOneShot.Play();
        await Task.Delay(TimeSpan.FromSeconds(0.5f), _tokenSource1.Token);
        spaceship.DOScale(new Vector3(0, 0, 0), 2);
        spaceship.DOMove(new Vector3(0, -20f, -0.2f), 2);
        await Task.Delay(TimeSpan.FromSeconds(2f), _tokenSource1.Token);
        TransitionManager.Instance.FadeToNextScene(sceneName);
      }
      catch (Exception)
      {
        Helper.Log("[IntroductionScene] Cancelled.");
      }
    }

    private async Task PlayAudio(Tween sequence)
    {
      try
      {
        while (sequence.IsPlaying())
        {
          AudioDirector.Instance.Play("Type");
          await Task.Delay(TimeSpan.FromSeconds(0.03f), _tokenSource2.Token);
        }
      }
      catch (Exception)
      {
        Helper.Log("[IntroductionScene] Cancelled.");
      }
    }

    private void OnDestroy()
    {
      _tokenSource1?.Cancel();
      _tokenSource2?.Cancel();
    }
  }
}