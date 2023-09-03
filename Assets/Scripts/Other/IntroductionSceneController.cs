using System;
using System.Threading;
using CaptainHindsight.Core;
using CaptainHindsight.Directors;
using CaptainHindsight.Directors.Audio;
using CaptainHindsight.Managers;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Task = System.Threading.Tasks.Task;

namespace CaptainHindsight.Other
{
  public class IntroductionSceneController : MonoBehaviour
  {
    [SerializeField, Required] private Transform holder;
    [SerializeField, Required] private GameObject instructions;
    [SerializeField, Required] private TextMeshProUGUI textMesh;
    [SerializeField, Required] private Transform scrollDestination;
    [SerializeField] private int textScrollDuration = 40;
    [SerializeField, Required] private Transform spaceship;
    [SerializeField] private int flyDuration = 15;
    [SerializeField, Required] private AudioOneShot audioOneShot;
    [SerializeField, Required] private TextMeshProUGUI[] paragraphs;
    [SerializeField, Required] private string sceneName;
    private PlayerInputActions _playerInputActions;
    private CancellationTokenSource _tokenSource;
    private bool _canSkipNow;

    private async void Awake()
    {
      instructions.gameObject.SetActive(false);
      _playerInputActions = new PlayerInputActions();
      _playerInputActions.Menu.Enable();
      _playerInputActions.Menu.Continue.performed += Continue;

      DOTween.SetTweensCapacity(1250, 50);
      foreach (var paragraph in paragraphs)
      {
        paragraph.DOFade(0, 0);
      }

      holder.DOMove(scrollDestination.position, textScrollDuration);
      spaceship.DOMove(new Vector3(0, 2.1f, -0.3f), flyDuration);
      _tokenSource = new CancellationTokenSource();
      foreach (var paragraph in paragraphs)
      {
        var sequence = AnimateParagraph(paragraph);
        await PlayAudio(sequence);
        await WaitOneSecond();
      }

      textMesh.text = "- Press any key to continue -";
      instructions.gameObject.SetActive(true);
      _canSkipNow = true;
    }

    private void Continue(InputAction.CallbackContext context)
    {
      if (_canSkipNow == false)
      {
        textMesh.text = "- Press any key to skip -";
        instructions.gameObject.SetActive(true);
        _canSkipNow = true;
        return;
      }

      spaceship.DOKill();
      _tokenSource?.Cancel();
      _tokenSource = new CancellationTokenSource();
      FadeToNextScene();
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
      catch (Exception)
      {
        Helper.Log("[IntroductionScene] Cancelled.");
        return null;
      }
    }

    private async Task WaitOneSecond()
    {
      try
      {
        await Task.Delay(TimeSpan.FromSeconds(1f), _tokenSource.Token);
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
        _playerInputActions.Menu.Continue.performed -= Continue;
        instructions.gameObject.SetActive(false);
        audioOneShot.Play();
        await Task.Delay(TimeSpan.FromSeconds(0.5f), _tokenSource.Token);
        spaceship.DOScale(new Vector3(0, 0, 0), 2);
        spaceship.DOMove(new Vector3(0, -20f, -0.2f), 2);
        AudioDirector.Instance.Play("Boom");
        await Task.Delay(TimeSpan.FromSeconds(2f), _tokenSource.Token);
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
          await Task.Delay(TimeSpan.FromSeconds(0.03f), _tokenSource.Token);
        }
      }
      catch (Exception)
      {
        Helper.Log("[IntroductionScene] Cancelled.");
      }
    }

    private void OnDestroy()
    {
      _tokenSource?.Cancel();
      _playerInputActions.Menu.Continue.performed -= Continue;
      _playerInputActions.Menu.Disable();
    }
  }
}