using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHindsight.Core;
using CaptainHindsight.Directors;
using CaptainHindsight.Directors.Audio;
using CaptainHindsight.Managers;
using CaptainHindsight.Other;
using CaptainHindsight.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CaptainHindsight.Player
{
  [DisallowMultipleComponent]
  public class PlayerManager : GameStateBehaviour, IDamageable, IHealable, IPlayerPrefsSaveable
  {
    private static PlayerManager _instance;

    [Title("Health")]
    [ShowInInspector]
    [ReadOnly]
    [ProgressBar(0, "MaxHealth", Height = 25)]
    [PropertySpace(SpaceBefore = 10, SpaceAfter = 10)]
    private int _currentHealth;

    private bool _canBeAffected = true;

    [SerializeField] [Min(1)] private int maxHealthBase = 100;
    [SerializeField] [Min(0)] private int maxHealthModifier;


#pragma warning disable 414
    private int MaxHealth => maxHealthBase + maxHealthModifier;
#pragma warning restore 414


    [Title("Effects")] [SerializeField] private Transform effectPopup;

    [SerializeField] private Transform spawnPosition;

    private MHealthBar _healthBar;
    [SerializeField] private AudioOneShot healSound;
    [SerializeField] private AudioOneShotRandom damageSound;
    [SerializeField] private AudioOneShot deathSound;

    [Title("Skills")] [ShowInInspector] [ReadOnly]
    private List<Skill> _activeSkills = new();

    #region Awake & Start

    private void Awake()
    {
      if (_instance == null)
      {
        _instance = this;
      }
      else
      {
        Destroy(gameObject);
      }
    }

    private void Start()
    {
      // Find health bar
      if (GameObject.Find("UI").GetComponentInChildren<MHealthBar>(true) == null)
        Debug.LogError("[PlayerManager] UI canvas not found.");
      else _healthBar = GameObject.Find("UI").GetComponentInChildren<MHealthBar>(true);

      // Initial configuration
      TryLoadPlayerPrefs();
      _healthBar.ConfigureBar(maxHealthBase + maxHealthModifier);
      _healthBar.SetHealth(_currentHealth);
    }

    #endregion

    #region Managing damage/health

    public void TakeDamage(int damage, Transform origin)
    {
      // Guard clause to prevent damage in certain GameStates
      if (_canBeAffected == false) return;

      // Calculate and set new health
      var newHealth = _currentHealth - damage;
      _currentHealth = newHealth;

      // Play damage sound
      damageSound.Play();

      // Update health bar
      _healthBar.SetHealth(Mathf.Clamp(newHealth, 0, maxHealthBase + maxHealthModifier));

      // Instantiate effect popup showing amount of damage taken
      var popup = Instantiate(effectPopup, transform);
      popup.position = spawnPosition.position;
      popup.GetComponent<MEffectPopup>().Initialisation(ActionType.Damage, "-" + damage);

      // Initiate death, if health <= 0
      if (newHealth <= 0) Die();
    }

    public void AddHealth(int health)
    {
      // Guard clause to prevent healing in certain GameStates
      if (_canBeAffected == false) return;

      // Guard clause to prevent over-healing
      if (_currentHealth == maxHealthBase + maxHealthModifier) return;

      // Calculate and set new health
      var newHealth = Mathf.Clamp(_currentHealth + health, 0, maxHealthBase + maxHealthModifier);
      _currentHealth = newHealth;

      // Update health bar
      _healthBar.SetHealth(newHealth);

      // Instantiate effect popup showing amount of damage taken
      healSound.Play();
      var popup = Instantiate(effectPopup, transform);
      popup.position = spawnPosition.position;
      popup.GetComponent<MEffectPopup>().Initialisation(ActionType.Health, "+" + health);
    }

    #endregion

    private async void Die()
    {
      deathSound.Play();
      _healthBar.gameObject.SetActive(false);
      GameStateDirector.Instance.SwitchState(GameState.GameOver);
      await Task.Delay(System.TimeSpan.FromSeconds(1f));
      Destroy(gameObject);
    }

    #region Managing events

    protected override void ActionGameStateChange(GameState state, GameStateSettings settings,
      string message)
    {
      _canBeAffected = settings.PlayerIsAffected;
    }

    private void ActionActiveSkillChange()
    {
      // Note that the activeSkills list only contains the highest skill of any type so that
      // GetActiveSkills(Skill.SkillType.MaxHealth) will only return the highest MaxHealth skill
      var highestRelevantSkill =
        PlayerSkillsManager.Instance.GetActiveSkills(Skill.SkillType.MaxHealth);
      if (highestRelevantSkill.Count == 0 ||
          _activeSkills.Contains(highestRelevantSkill[0])) return;
      _activeSkills.Add(highestRelevantSkill[0]);
      maxHealthModifier = (int)highestRelevantSkill[0].Value;
      _healthBar.SetMaxHealth(maxHealthBase + maxHealthModifier);
    }

    protected override void OnEnable()
    {
      base.OnEnable();
      EventManager.Instance.OnActiveSkillsChange += ActionActiveSkillChange;
      EventManager.Instance.OnSceneExit += TrySavePlayerPrefs;
    }

    protected override void OnDestroy()
    {
      base.OnDestroy();
      EventManager.Instance.OnActiveSkillsChange -= ActionActiveSkillChange;
      EventManager.Instance.OnSceneExit -= TrySavePlayerPrefs;
    }

    public void TryLoadPlayerPrefs()
    {
      _currentHealth = PlayerPrefsManager.Instance.LoadInt(GlobalConstants.CURRENT_HEALTH);
      maxHealthModifier = PlayerPrefsManager.Instance.LoadInt(GlobalConstants.CURRENT_MAX_HEALTH);
      if (_currentHealth == 0) _currentHealth = maxHealthBase + maxHealthModifier;
    }

    public void TrySavePlayerPrefs(string spawnPointName)
    {
      PlayerPrefsManager.Instance.Save(GlobalConstants.CURRENT_HEALTH, _currentHealth);
      PlayerPrefsManager.Instance.Save(GlobalConstants.CURRENT_MAX_HEALTH, maxHealthModifier);
    }

    #endregion
  }
}