using System.Collections.Generic;
using CaptainHindsight.Core;
using CaptainHindsight.Core.Observer;
using CaptainHindsight.Managers;
using CaptainHindsight.Other;
using CaptainHindsight.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.NPCs
{
  [DisallowMultipleComponent]
  public class HealthManager : MonoBehaviour, IDamageable, IObservable
  {
    [BoxGroup("Name")] [SerializeField] [HideLabel] [PropertySpace(SpaceBefore = 5, SpaceAfter = 5)]
    private string npcName = "Unspecified";

    [BoxGroup("Base Stats")]
    [Title("Current Health", bold: false, horizontalLine: false,
      titleAlignment: TitleAlignments.Centered)]
    [GUIColor(0.3f, 0.8f, 0.8f, 1f)]
    [HideLabel]
    [ShowInInspector]
    [ReadOnly]
    [ProgressBar(0, "maxHealth", ColorGetter = "GetHealthBarColor", Height = 25)]
    [PropertySpace(SpaceBefore = -10, SpaceAfter = 10)]
    private int _currentHealth;

    [BoxGroup("Base Stats")] [SerializeField] [Min(1)]
    private int maxHealth = 100;

    [BoxGroup("Base Stats")]
    [SerializeField]
    [Min(1)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 5)]
    private int experienceReleased = 5;

    [TabGroup("Particles")]
    [Required]
    [SerializeField]
    [AssetSelector(Paths = "Assets/Prefabs/Combat", DropdownWidth = 300)]
    private GameObject meatParticles;

    [TabGroup("Particles")]
    [Required]
    [SerializeField]
    [AssetSelector(Paths = "Assets/Prefabs/Combat", DropdownWidth = 300)]
    private GameObject bloodParticles;

    [TabGroup("Particles")]
    [Required]
    [SerializeField]
    [AssetSelector(Paths = "Assets/Prefabs/Combat", DropdownWidth = 300)]
    private GameObject bloodSplashParticles;

    [TabGroup("UI")] [Required] [SerializeField]
    private MHealthBar healthBar;

    [TabGroup("UI")] [Required] [SerializeField]
    private Transform uiPositionMarker;

    [TabGroup("UI")]
    [Required]
    [SerializeField]
    [AssetSelector(Paths = "Assets/Prefabs/UI", DropdownWidth = 300)]
    private Transform effectPopup;

    [TabGroup("UI")] [Required] [SerializeField]
    private Transform spawnPosition;

    [FoldoutGroup("Deactivate OnDeath", false)] [SerializeField] [Required]
    private Animator animator;

    [FoldoutGroup("Deactivate OnDeath", false)] [SerializeField] [Required]
    private Npc stateMachine;

    [FoldoutGroup("Deactivate OnDeath", false)] [SerializeField] [Required]
    private Transform awarenessTransform;

    [FoldoutGroup("Deactivate OnDeath", false)] [SerializeField] [Required]
    private Transform actionTransform;

    [FoldoutGroup("Deactivate OnDeath", false)] [SerializeField] [Required]
    private Transform particleTransform;

    [FoldoutGroup("Observers", true)] [ShowInInspector] [ReadOnly]
    private List<Observer> _observers = new();

    [HideInInspector] public FactionIdentity factionIdentity;

    private void Awake()
    {
      _currentHealth = maxHealth;
      healthBar.ConfigureBar(maxHealth, npcName);
      factionIdentity = GetComponent<FactionIdentity>();
    }

    public void TakeDamage(int damage, Transform origin)
    {
      // Guard clauses
      if (_currentHealth <= 0) return;
      if (factionIdentity.GetFaction() ==
          origin.GetComponent<FactionIdentity>().GetFaction()) return;

      // Calculate and set new health
      var newHealth = _currentHealth - damage;
      _currentHealth = newHealth;

      // Update health bar
      healthBar.SetHealth(Mathf.Clamp(newHealth, 0, maxHealth));

      // Instantiate effect popup showing amount of damage taken
      var popup = Instantiate(effectPopup, transform);
      popup.position = spawnPosition.position;
      popup.GetComponent<MEffectPopup>()
        .Initialisation(ActionType.Positive, "-" + damage.ToString());

      // Instantiate blood effect
      Instantiate(meatParticles, transform.position, Quaternion.identity);
      Instantiate(bloodParticles, transform.position, Quaternion.identity);
      Instantiate(bloodSplashParticles, particleTransform.position, Quaternion.identity);

      // Initiate death if health <= 0 or consider taking action
      if (newHealth <= 0) Die(origin);
      else stateMachine.TakeActionAfterReceivingDamage(origin);
    }

    public void AddHealth(int health)
    {
      // Calculate and set new health
      var newHealth = Mathf.Clamp(_currentHealth + health, 0, maxHealth);
      _currentHealth = newHealth;

      // Update health bar
      healthBar.SetHealth(newHealth);

      // Instantiate effect popup showing amount of damage taken
      var popup = Instantiate(effectPopup, transform);
      popup.position = spawnPosition.position;
      popup.GetComponent<MEffectPopup>()
        .Initialisation(ActionType.Negative, "+" + health.ToString());
    }

    public void RegisterObserver(Observer observer)
    {
      _observers.Add(observer);
    }

    private void Die(Transform origin)
    {
      foreach (var observer in _observers)
        if (observer is OriginObserver originObserver)
          originObserver.ProcessInformation(origin);
        else observer.ProcessInformation();

      EventManager.Instance.EnemyDies(origin, experienceReleased);

      healthBar.gameObject.SetActive(false);
      stateMachine.Die();
      actionTransform.gameObject.SetActive(false);
      awarenessTransform.gameObject.SetActive(false);
      particleTransform.gameObject.SetActive(false);
      Destroy(gameObject);
    }
  }
}