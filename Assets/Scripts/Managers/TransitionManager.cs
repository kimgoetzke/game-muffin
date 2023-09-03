using System.Threading.Tasks;
using CaptainHindsight.Core;
using CaptainHindsight.Directors;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CaptainHindsight.Managers
{
  public class TransitionManager : MonoBehaviour
  {
    public static TransitionManager Instance;

    [SerializeField] [Required] private Image blackImage;

    [SerializeField] private float lengthOfFadeOut = 1f;

    [SerializeField] private float lengthOfFadeIn = 3f;

    private void Awake()
    {
      blackImage.gameObject.SetActive(true);

      if (Instance == null)
      {
        Instance = this;
      }
      else
      {
        Destroy(gameObject);
      }
    }

    public void FadeToNextScene(string levelName)
    {
      LoadLevelAfterFade(levelName, 999);
    }

    public void FadeToNextScene(int levelNumber)
    {
      LoadLevelAfterFade("", levelNumber);
    }

    private async void OnEnable()
    {
      AudioDirector.Instance.PlayBackgroundMusic();
      blackImage
        .DOFade(0f, lengthOfFadeIn)
        .SetEase(Ease.InSine)
        .SetUpdate(UpdateType.Normal, true);
      await Task.Delay(System.TimeSpan.FromSeconds(lengthOfFadeIn));

      // Switch to Play state
      GameStateDirector.Instance.SwitchState(GameState.Play);
    }

    private async void LoadLevelAfterFade(string sceneName, int sceneNumber)
    {
      blackImage
        .DOFade(1f, lengthOfFadeOut)
        .SetUpdate(UpdateType.Normal, true);
      
      GameStateDirector.Instance.SwitchState(GameState.Transition);
      await Task.Delay(System.TimeSpan.FromSeconds(lengthOfFadeOut + 1f));
      
      DOTween.KillAll();
      if (sceneNumber == 999)
      {
        SceneManager.LoadScene(sceneName);
      }
      else
      {
        SceneManager.LoadScene(sceneNumber);
      }
    }
  }
}