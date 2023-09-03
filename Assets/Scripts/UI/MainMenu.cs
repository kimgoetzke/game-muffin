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
      var sceneName = PlayerPrefsManager.Instance.LoadString(GlobalConstants.ACTIVE_SCENE);
      Helper.Log("[MainMenu] Loading scene: " + sceneName + ".");
      TransitionManager.Instance.FadeToNextScene(sceneName);
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