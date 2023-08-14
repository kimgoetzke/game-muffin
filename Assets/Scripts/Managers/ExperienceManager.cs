using CaptainHindsight.Core;
using CaptainHindsight.Directors;
using CaptainHindsight.Other;
using CaptainHindsight.Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Managers
{
  public class ExperienceManager : MonoBehaviour, IPlayerPrefsSaveable
  {
    public static ExperienceManager Instance;

    [BoxGroup("Basics")]
    [SerializeField]
    [ReadOnly]
    [PropertySpace(SpaceBefore = 10, SpaceAfter = 10)]
    private int currentLevel = 1;

    [BoxGroup("Basics")]
    [Title("Current Experience", bold: false, horizontalLine: false,
      titleAlignment: TitleAlignments.Centered)]
    [GUIColor(0.3f, 0.8f, 0.8f)]
    [HideLabel]
    [ShowInInspector]
    [ReadOnly]
    [ProgressBar(0, "_experienceToNextLevel", Height = 25)]
    [PropertySpace(SpaceBefore = -10, SpaceAfter = 10)]
    private int _currentExperience;

    [BoxGroup("Basics")] private int _experienceToNextLevel;

    private Transform _player; // Only used in Editor mode

    [BoxGroup("Other")] [SerializeField] private Transform effectPopup;

    [BoxGroup("Other")] [ShowInInspector] [ReadOnly]
    private static readonly int[] ExperiencePerLevel = new[] { 10, 20, 40, 60, 85, 115, 150 };

    #region Managing Unity Editor-only buttons

    [TitleGroup("Editor actions")]
    [HorizontalGroup("Editor actions/Split")]
    [VerticalGroup("Editor actions/Split/Left")]
    [Button("Add experience", ButtonSizes.Large)]
    [GUIColor(0, 0.9f, 0)]
    private void AddExperienceViaEditorButton()
    {
      if (!Application.isPlaying) return;
      if (_player == null) _player = GameObject.Find("Player").transform;
      AddExperience(_player, 5);
    }

    [VerticalGroup("Editor actions/Split/Right")]
    [Button("Level up", ButtonSizes.Large)]
    [GUIColor(0, 0.9f, 0)]
    private void AddLevelViaEditorButton()
    {
      if (!Application.isPlaying) return;
      if (_player == null) _player = GameObject.Find("Player").transform;
      LevelUp(_player);
    }

    #endregion

    private void Awake()
    {
      if (Instance == null)
      {
        Instance = this;
      }
      else
      {
        Destroy(gameObject);
        return;
      }

      _experienceToNextLevel = GetExperienceToNextLevel(currentLevel);
    }

    private void Start() => TryLoadPlayerPrefs();

    private int GetExperienceToNextLevel(int level)
    {
      return level - 1 < ExperiencePerLevel.Length
        ? ExperiencePerLevel[level - 1]
        : ExperiencePerLevel[^2];
    }

    public int GetCurrentLevel()
    {
      return currentLevel;
    }

    #region Managing events (incl. own events)

    private void ActionEnemyDeathEvent(Transform causedBy, int experience)
    {
      if (causedBy.CompareTag("Player"))
        AddExperience(causedBy, experience);
    }

    private void AddExperience(Transform causedBy, int experience)
    {
      // For debugging:
      // Helper.Log($"[ExperienceManager] Player gained {experience} experience.");
      _currentExperience += experience;
      if (_currentExperience >= _experienceToNextLevel)
      {
        _currentExperience -= _experienceToNextLevel;
        LevelUp(causedBy);
      }

      EventManager.Instance.ChangeExperience(_currentExperience, _experienceToNextLevel);
    }

    private void LevelUp(Transform causedBy)
    {
      AudioDirector.Instance.Play("LevelUp");
      currentLevel++;
      _experienceToNextLevel = GetExperienceToNextLevel(currentLevel);
      EventManager.Instance.ChangeSkillPoints(1);
      SpawnEffectPopup(causedBy);
    }

    private void SpawnEffectPopup(Transform spawnPosition)
    {
      var popup = Instantiate(effectPopup, spawnPosition);
      var pos = spawnPosition.position;
      popup.position = new Vector3(pos.x, pos.y + 0.75f, pos.z + 0.75f);
      popup.GetComponent<MEffectPopup>().Initialisation(ActionType.Other, "Level up!");
    }

    private void OnEnable()
    {
      EventManager.Instance.OnEnemyDeath += ActionEnemyDeathEvent;
      EventManager.Instance.OnSceneExit += TrySavePlayerPrefs;
    }

    private void OnDestroy()
    {
      EventManager.Instance.OnEnemyDeath -= ActionEnemyDeathEvent;
      EventManager.Instance.OnSceneExit -= TrySavePlayerPrefs;
    }

    #endregion

    #region Managing saving and loading

    public void TryLoadPlayerPrefs()
    {
      currentLevel = Mathf.Max(PlayerPrefsManager.Instance.LoadInt(GlobalConstants.CURRENT_LEVEL),
        currentLevel);
      _currentExperience = PlayerPrefsManager.Instance.LoadInt(GlobalConstants.CURRENT_EXPERIENCE);
      _experienceToNextLevel = GetExperienceToNextLevel(currentLevel);
      EventManager.Instance.ChangeExperience(_currentExperience, _experienceToNextLevel);
    }

    public void TrySavePlayerPrefs(string spawnPointName)
    {
      PlayerPrefsManager.Instance.Save(GlobalConstants.CURRENT_LEVEL, currentLevel);
      PlayerPrefsManager.Instance.Save(GlobalConstants.CURRENT_EXPERIENCE, _currentExperience);
    }

    #endregion
  }
}