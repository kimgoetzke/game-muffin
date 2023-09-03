using System;
using System.Threading;
using CaptainHindsight.Core;
using CaptainHindsight.Directors;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace CaptainHindsight.Other
{
  public class MTextOnScreen : MonoBehaviour
  {
    [SerializeField, Required] private Transform holder;
    [SerializeField, Required] private Transform destination;
    [SerializeField, Required] private TextMeshProUGUI[] paragraphs;
    private CancellationTokenSource _tokenSource;

    private void Awake()
    {
      foreach (var paragraph in paragraphs)
      {
        paragraph.color = new Color(paragraph.color.r, paragraph.color.g, paragraph.color.b, 0);
        paragraph.gameObject.SetActive(false);
      }
    }

    private async void Start()
    {
      _tokenSource = new CancellationTokenSource();
      foreach (var paragraph in paragraphs)
      {
        paragraph.gameObject.SetActive(true);
      }

      holder.DOMove(destination.position, 15f);
      foreach (var paragraph in paragraphs)
      {
        var animator = new DOTweenTMPAnimator(paragraph);
        var sequence = DOTween.Sequence().Pause();
        sequence = BuildSequence(animator, sequence);
        await Task.Delay(TimeSpan.FromSeconds(0.2f), _tokenSource.Token);
        await Play(sequence);
        await Wait();
      }
    }

    private Sequence BuildSequence(DOTweenTMPAnimator animator, Sequence sequence)
    {
      try
      {
        for (var i = 0; i < animator.textInfo.characterCount; i++)
        {
          if (animator.textInfo.characterInfo[i].isVisible == false) continue;
          sequence
            .Append(animator.DOFadeChar(i, 1, 0.02f))
            .Append(animator.DOPunchCharScale(i, new Vector3(1.1f, 1.1f, 1.1f), 0.02f));
        }

        return sequence;
      }
      catch (Exception)
      {
        Helper.Log("[IntroductionScene] AnimateParagraph cancelled.");
        return null;
      }
    }

    private async Task Wait(int seconds = 1)
    {
      try
      {
        await Task.Delay(TimeSpan.FromSeconds(seconds), _tokenSource.Token);
      }
      catch (Exception)
      {
        Helper.Log("[IntroductionScene] WaitOneSecond cancelled.");
      }
    }

    private async Task Play(Tween sequence)
    {
      try
      {
        sequence.Play();
        while (sequence.IsPlaying())
        {
          AudioDirector.Instance.Play("Type");
          await Task.Delay(TimeSpan.FromSeconds(0.03f), _tokenSource.Token);
        }
      }
      catch (Exception)
      {
        Helper.Log("[IntroductionScene] PlayAudio cancelled.");
      }
    }

    private void OnDisable()
    {
      _tokenSource?.Cancel();
    }
  }
}