using CaptainHindsight.Data.Skills;
using CaptainHindsight.Directors;
using CaptainHindsight.Managers;
using CaptainHindsight.Player;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CaptainHindsight.UI
{
  public class SkillMenuElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
  {
    [TitleGroup("Grid element settings")] [SerializeField] [Required]
    private Image iconImage;

    [SerializeField] [Required] private Image borderImage;
    [SerializeField] private Color availableColour;
    [SerializeField] private Color activeColour;
    [SerializeField] private Color lockedColour;
    [ShowInInspector] [ReadOnly] private Skill _skill;
    [ShowInInspector] [ReadOnly] private bool _isActive;
    [ShowInInspector] [ReadOnly] private bool _isLocked;
    private PlayerSkillsManager _playerSkillsManager;

    [TitleGroup("Skill details settings")] [SerializeField] [Required]
    private GameObject skillDetails;

    [ShowInInspector] [ReadOnly] private bool _isHovering;


    public void Initialise(Skill skill)
    {
      _skill = skill;
      iconImage.sprite = skill.Icon;
      _playerSkillsManager = PlayerSkillsManager.Instance;
      _isActive = _playerSkillsManager.IsSkillUnlocked(skill);
      _isLocked = _playerSkillsManager.CanUnlock(skill) == false;
      iconImage.color = new Color(1, 1, 1, _isActive ? 1 : 0.65f);
      borderImage.color = _isActive ? activeColour : availableColour;
      borderImage.color = _isLocked ? lockedColour : borderImage.color;
      InitialiseSkillDetails();
    }

    public void RequestUnlock()
    {
      if (_isActive)
        // Helper.Log("[SkillMenuElement] No action taken. Skill already active.");
        return;

      // Helper.Log("[SkillMenuElement] Attempting to unlock skill.");
      if (_playerSkillsManager.TryUnlockAndActivateSkill(_skill))
      {
        ActivateSkill();
        AudioDirector.Instance.Play("Positive");
        // TODO: Add unlock particle effect and/or animation
      }
      else
      {
        // Helper.Log("[SkillMenuElement] Failed to unlock skill.");
        AudioDirector.Instance.Play("Cancel");
        transform.DOKill(true);
        transform.DOShakePosition(0.3f, 5, 10, 0)
          .SetUpdate(UpdateType.Normal, true);
      }
    }

    private void ActivateSkill()
    {
      iconImage.color = new Color(1, 1, 1, 1);
      borderImage.color = activeColour;
      _isActive = true;
      _isLocked = false;
      UpdateSkillDetails();
    }

    #region Managing SkillDetails tooltip

    public void OnPointerEnter(PointerEventData eventData)
    {
      skillDetails.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
      skillDetails.SetActive(false);
    }

    private void InitialiseSkillDetails()
    {
      skillDetails.GetComponent<SkillsDetails>().SetDetails(_skill, _isActive, _isLocked,
        _playerSkillsManager.HasSkillPoints());
      skillDetails.SetActive(false);
    }

    private void UpdateSkillDetails()
    {
      skillDetails.GetComponent<SkillsDetails>()
        .SetDetails(_skill, _isActive, _isLocked, _playerSkillsManager.HasSkillPoints());
    }

    #endregion

    #region Managing events

    private void ActionAnyChange()
    {
      if (_isActive) return;
      if (_playerSkillsManager.CanUnlock(_skill))
      {
        // Helper.Log("[SkillMenuElement] '" + skill.Name + "' now available to unlock because skill requirement is now met.");
        MakeSkillAvailable();
        UpdateSkillDetails();
      }

      UpdateSkillDetails();
    }

    private void MakeSkillAvailable()
    {
      iconImage.color = new Color(1, 1, 1, 0.65f);
      borderImage.color = availableColour;
      _isLocked = false;
    }

    private void ActionSkillPointsChange(int change)
    {
      if (change > 0)
        ActionAnyChange();
    }

    private void OnEnable()
    {
      EventManager.Instance.OnActiveSkillsChange += ActionAnyChange;
      EventManager.Instance.OnSkillPointChange += ActionSkillPointsChange;
    }

    private void OnDestroy()
    {
      EventManager.Instance.OnActiveSkillsChange -= ActionAnyChange;
      EventManager.Instance.OnSkillPointChange -= ActionSkillPointsChange;
    }

    #endregion
  }
}