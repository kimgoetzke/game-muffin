using CaptainHindsight.Core;
using CaptainHindsight.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.UI
{
  public class MainMenu : MonoBehaviour
  {
    [SerializeField] [Required] private GameObject continueButton;

    private void Awake()
    {
      continueButton.SetActive(PlayerPrefs.HasKey(GlobalConstants.PREVIOUS_SCENE_EXIT_SPAWN));
    }

    public void ContinueGame()
    {
      TransitionManager.Instance.FadeToNextScene(GlobalConstants.PREVIOUS_SCENE);
    }

    public void NewGame()
    {
      PlayerPrefs.DeleteAll(); // Will need to rework if Option Menu is added
      TransitionManager.Instance.FadeToNextScene(1);
    }

    public void QuitGame()
    {
      Helper.Log("Application closed.");
      Application.Quit();
    }
  }
}