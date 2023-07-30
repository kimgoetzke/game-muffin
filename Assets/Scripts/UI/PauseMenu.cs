using System.Threading.Tasks;
using CaptainHindsight.Core;
using CaptainHindsight.Data.GameStates;
using CaptainHindsight.Directors;
using CaptainHindsight.Managers;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CaptainHindsight.UI
{
  public class PauseMenu : GameStateBehaviour, IMenuNestable
  {
    [Title("General configuration")] [SerializeField] [Required]
    public GameObject pauseMenuUI;

    [SerializeField] [Required] private GraphicRaycaster raycaster;

    [ShowInInspector] [ReadOnly] private bool _allowButtonPress = true;

    [SerializeField] [ListDrawerSettings(ShowFoldout = true)] [ChildGameObjectsOnly]
    private RectTransform[] buttons;

    [HideInInspector] private bool _gameIsPaused = false;

    [Title("Game Over menu")] [SerializeField] [Required]
    private TextMeshProUGUI titleMesh;

    [SerializeField] [Required] private TextMeshProUGUI subTitleMesh;
    [SerializeField] [Required] private GameObject resumeButton;
    [SerializeField] [Required] private GameObject tryAgainButton;

    [Title("Other menus")] [SerializeField] [Required]
    private GameObject skillButton;

    [SerializeField] [Required] public BaseSubMenu skillMenu;

    private void Awake()
    {
      subTitleMesh.gameObject.SetActive(false);
      tryAgainButton.SetActive(false);
    }

    #region Game Over menu

    private async void SwitchToAndTriggerGameOverMenu(string message)
    {
      // Lock pause menu so it cannot be triggered during death animation and
      // it doesn't need to be unlocked later as there will be a change in scene
      LockPauseMenu(true);

      // Change pause menu title to game over menu title
      titleMesh.text = " -GAME OVER-";
      subTitleMesh.gameObject.SetActive(true);
      subTitleMesh.text = message == "" ? "You died." : message;

      // Deactivate/activate relevant buttons
      skillButton.SetActive(false);
      tryAgainButton.SetActive(true);
      resumeButton.GetComponent<Button>().interactable = false;
      resumeButton.GetComponentInChildren<TextMeshProUGUI>().alpha = 0.2f;

      // Wait 2 seconds, then trigger menu
      await Task.Delay(System.TimeSpan.FromSeconds(2f));
      AudioDirector.Instance.StopBackgroundMusic();
      AudioDirector.Instance.Play("GameOver");
      PauseGame();
    }

    #endregion

    #region Managing button functionality

    private void PauseGame()
    {
      // Ensure that the below, esp. animations, cannot be interrupted/messed
      // up by going out of the pause menu
      LockPauseMenu(true);

      // Enable raycaster so that player can press buttons
      raycaster.enabled = true;

      // Play sound effect
      PlayButtonPressSound();

      // Fade in background and title
      pauseMenuUI.SetActive(true);
      pauseMenuUI.GetComponent<Image>().DOFade(0.8f, 0.4f).SetUpdate(UpdateType.Normal, true)
        .OnComplete(() => { titleMesh.gameObject.SetActive(true); });

      // Animations that move each button into the visible screen
      MoveButtonsIntoView();

      // Allowing user to use Escape again
      LockPauseMenu(false);
    }

    private async Task ResumeGame()
    {
      // Ensure that the below, esp. animations, cannot be interrupted/messed
      // up by going out of the pause menu
      LockPauseMenu(true);

      // Disable raycaster again so it doesn't interfere
      raycaster.enabled = false;

      // Play sound effect
      PlayButtonPressSound();

      // Animations that move each button out of the visible screen
      MoveButtonsOutOfView();

      // Hide the title and fade out background
      titleMesh.gameObject.SetActive(false);
      pauseMenuUI.GetComponent<Image>().DOFade(0f, 0.7f).SetUpdate(UpdateType.Normal, true);

      // Add a delay to keep menu locked during animation
      await Task.Delay(System.TimeSpan.FromSeconds(0.7f));

      pauseMenuUI.SetActive(false);

      // Allowing user to use Escape again
      LockPauseMenu(false);

      await Task.Yield();
    }

    public async void SkillMenu()
    {
      // Ensure that the below, esp. animations, cannot be interrupted/messed
      // up by going out of the pause menu - IMPORTANT:
      // The pause menu remains locked until the user is back from the sub-menu.
      LockPauseMenu(true);

      // Play sound effect
      PlayButtonPressSound();

      // Animations that move each button out of the visible screen
      var sequence = DOTween.Sequence().SetUpdate(UpdateType.Normal, true);
      foreach (var button in buttons)
        sequence.Append(button.DOAnchorPosY(-60, 0.2f).SetEase(Ease.Linear));

      // Hide the title (but keep the background)
      titleMesh.gameObject.SetActive(false);
      raycaster.enabled = false;

      // Add a delay to keep menu locked during animation
      await Task.Delay(System.TimeSpan.FromSeconds(0.5f));

      // Swap active menu screens
      pauseMenuUI.SetActive(false);
      skillMenu.OpenMenu(this);
    }

    public void RestartLevel()
    {
      TransitionManager.Instance.FadeToNextScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
      TransitionManager.Instance.FadeToNextScene("MainMenu");
    }

    #endregion

    #region Managing audio

    public void PlayButtonHighlightSound()
    {
      AudioDirector.Instance.Play("Select");
    }

    public void PlayButtonPressSound()
    {
      AudioDirector.Instance.Play("Click");
    }

    #endregion

    private void LockPauseMenu(bool status)
    {
      _allowButtonPress = !status;
    }

    private void MoveButtonsIntoView()
    {
      var sequence = DOTween.Sequence().SetUpdate(UpdateType.Normal, true);
      foreach (var button in buttons)
        if (button.gameObject.activeSelf == true)
          sequence.Append(button.DOAnchorPosY(25, 0.2f).SetEase(Ease.Linear));
    }

    private void MoveButtonsOutOfView()
    {
      var sequence = DOTween.Sequence().SetUpdate(UpdateType.Normal, true);
      foreach (var button in buttons)
        if (button.gameObject.activeSelf == true)
          sequence.Append(button.DOAnchorPosY(-60, 0.2f).SetEase(Ease.Linear));
    }

    public void ReturnFromSubMenu()
    {
      pauseMenuUI.SetActive(true);
      raycaster.enabled = true;
      titleMesh.gameObject.SetActive(true);
      MoveButtonsIntoView();
      LockPauseMenu(false);
    }

    #region Managing events and game states

    // Method used by EventManager (to block escape key when in other menus, for example)
    // and while menu fade in animations are playing
    public async void AttemptToPauseOrUnpause()
    {
      if (_allowButtonPress)
      {
        if (_gameIsPaused)
        {
          await ResumeGame();
          _gameIsPaused = false;
          GameStateDirector.Instance.SwitchState(GameState.Play);
        }
        else if (_gameIsPaused == false)
        {
          PauseGame();
          _gameIsPaused = true;
          GameStateDirector.Instance.SwitchState(GameState.Pause);
        }
      }
      else
      {
        Helper.LogWarning("PauseMenu: Menu is locked. Try again later.");
      }
    }

    protected override void ActionGameStateChange(GameState state, GameStateSettings settings,
      string message)
    {
      Helper.Log($"[PauseMenu] GameState changed to {state} with " +
                 (message == "" ? "no message" : "'" + message + "'") + ".");
      if (state == GameState.GameOver)
        SwitchToAndTriggerGameOverMenu(message);
      else if (state == GameState.Tutorial || state == GameState.Transition)
        LockPauseMenu(true);
      else if (state == GameState.Play) LockPauseMenu(false);
    }

    protected override void OnEnable()
    {
      base.OnEnable();
      EventManager.Instance.OnPauseMenuRequest += AttemptToPauseOrUnpause;
      raycaster.enabled =
        false; // Without this the pause menu will block interactions with other menus (such as an end of level menu) and possible a touch controller
    }

    protected override void OnDestroy()
    {
      base.OnDestroy();
      EventManager.Instance.OnPauseMenuRequest -= AttemptToPauseOrUnpause;
    }

    #endregion
  }
}