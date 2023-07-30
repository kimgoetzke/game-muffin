using CaptainHindsight.Data.Skills;
using CaptainHindsight.Managers;
using CaptainHindsight.Player;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace CaptainHindsight.UI
{
  public class SkillsDetails : MonoBehaviour
  {
    [SerializeField] [Required] private RectTransform holder;
    [SerializeField] [Required] private RectTransform background;
    [SerializeField] [Required] private TextMeshProUGUI title;
    [SerializeField] [Required] private TextMeshProUGUI status;
    [SerializeField] [Required] private TextMeshProUGUI description;
    [SerializeField] [Required] private GameObject levelRequirementsHolder;
    [SerializeField] [Required] private TextMeshProUGUI levelRequirements;
    [SerializeField] [Required] private RectTransform levelRequirementFailed;
    [SerializeField] [Required] private GameObject skillRequirementsHolder;
    [SerializeField] [Required] private TextMeshProUGUI skillRequirements;
    [SerializeField] [Required] private RectTransform skillRequirementFailed;
    [SerializeField] private Color activeColor;
    [SerializeField] private Color lockedColor;
    [SerializeField] private Color defaultColor;

    public void SetDetails(Skill skill, bool active, bool locked, bool skillPoints)
    {
      // Set basics
      title.text = skill.Name;
      description.text = skill.Description;
      if (active)
      {
        status.text = "Active";
        status.color = activeColor;
        levelRequirementsHolder.gameObject.SetActive(false);
        skillRequirementsHolder.gameObject.SetActive(false);
        return;
      }
      else if (locked)
      {
        status.text = "Locked";
        status.color = lockedColor;
      }
      else
      {
        status.text = skillPoints ? "1 skill point to unlock" : "Need skill point to unlock";
        status.color = skillPoints ? defaultColor : lockedColor;
      }

      // Set level requirement for available and locked skills
      if (skill.Requirements.hasLevelRequirement)
      {
        if (ExperienceManager.Instance.GetCurrentLevel() >= skill.Requirements.LevelRequirement)
          levelRequirementFailed.gameObject.SetActive(false);
        levelRequirements.text = skill.Requirements.LevelRequirement.ToString();
      }
      else
      {
        levelRequirements.text = "0";
        levelRequirementFailed.gameObject.SetActive(false);
      }

      // Set skill requirement for available and locked skills
      if (skill.Requirements.hasSkillRequirement)
      {
        if (PlayerSkillsManager.Instance.IsSkillUnlocked(skill.Requirements.SkillRequirement))
          skillRequirementFailed.gameObject.SetActive(false);
        skillRequirements.text = skill.Requirements.SkillRequirement.Name;
      }
      else
      {
        skillRequirements.text = "None";
        skillRequirementFailed.gameObject.SetActive(false);
      }
    }
  }
}