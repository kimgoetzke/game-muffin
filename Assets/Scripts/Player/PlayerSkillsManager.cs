using System.Collections.Generic;
using CaptainHindsight.Core;
using CaptainHindsight.Data.Skills;
using CaptainHindsight.Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using static CaptainHindsight.Data.Skills.Skill;

namespace CaptainHindsight.Player
{
  public class PlayerSkillsManager : MonoBehaviour, IPlayerPrefsSaveable
  {
    public static PlayerSkillsManager Instance;

    [TitleGroup("Base settings")]
    [InfoBox("You can add skill points only via the ExperienceManager.")]
    [SerializeField]
    [ReadOnly]
    private int skillPoints;

    [InfoBox(
      "Make sure to manually select all skills you want to make available in the list below. AutoPopulate is not working as intended.")]
    [AssetList(Path = "/Data/Skills/")]
    [SerializeField]
    private List<Skill> allSkills = new();

    // Unlocked skill are skills that have been unlocked by the player but are superseded
    // by a more skill, e.g. HealthMax1 and HealthMax2 - once the player unlocks HealthMax2,
    // HealthMax1 is no longer considered to be active.
    [SerializeField] private List<Skill> unlockedSkills = new();

    // Active skills are skills that are being used by other scripts, e.g. PlayerController,
    // EquipmentController. They are the skills that are actually affecting the player.
    [SerializeField] private List<Skill> activeSkills = new();

    #region Managing Unity Editor-only buttons

    [TitleGroup("Editor actions")]
    [Button("Activate and update all skills", ButtonSizes.Large)]
    [GUIColor(0, 1f, 0)]
    private void ActivateAllSkillsViaEditorButton()
    {
      if (Application.isPlaying)
      {
        if (allSkills.Count == 0)
        {
          Helper.LogWarning(
            "[PlayerSkillsManager] No skills found. Please refresh the skill list.");
          return;
        }

        var temp = skillPoints;
        skillPoints = allSkills.Count;
        activeSkills.Clear();
        unlockedSkills.Clear();
        allSkills.Sort((x, y) => x.Order.CompareTo(y.Order));
        allSkills.ForEach(s => TryUnlockAndActivateSkill(s));
        skillPoints = temp;
        Helper.Log(
          "[PlayerSkillsManager] Added all skills to active skills list and updated active skill list manually.");
      }
      else
      {
        Helper.LogWarning("[PlayerSkillsManager] Cannot activate all skills in editor mode.");
      }
    }

    [InfoBox(
      "The 'Update skills' button will update the active skills list. It will remove any skills that are superseded by a newer version of the same skill. It will also add any skills that are not already in the active skills list.")]
    [Button("Update skills", ButtonSizes.Large)]
    private void UpdateActiveSkillsViaEditorButton()
    {
      if (Application.isPlaying)
      {
        activeSkills.ForEach(s => RemoveDuplicatesInList(activeSkills, s));
        EventManager.Instance.UpdateActiveSkills();
      }
      else
      {
        Helper.LogWarning("[PlayerSkillsManager] Cannot update active skills list in editor mode.");
      }
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

      // Ensure that unlockedSkills and activeSkills are in sync for pre-set skills.
      // Helper.Log("[PlayerSkillsManager] " + unlockedSkills.Count + " unlocked skills found.");
      unlockedSkills.ForEach(s => TryActivateSkill(s));
    }

    private void Start()
    {
      TryLoadPlayerPrefs();
    }

    public bool HasSkillPoints()
    {
      return skillPoints > 0;
    }

    public int GetCurrentSkillPoints()
    {
      return skillPoints;
    }

    public bool IsSkillUnlocked(Skill skill)
    {
      return unlockedSkills.Contains(skill);
    }

    public List<Skill> GetAllSkills()
    {
      return allSkills;
    }

    public List<Skill> GetActiveSkills(SkillType skillType)
    {
      return activeSkills.FindAll(s => s.Type.Equals(skillType));
    }

    public List<Skill> GetActiveSkills(Equipment equipment)
    {
      return activeSkills.FindAll(s => s.EquipmentTarget.Equals(equipment));
    }

    public bool CanUnlock(Skill skill)
    {
      var levelRequirementMet = skill.Requirements
        .LevelRequirementMet(ExperienceManager.Instance.GetCurrentLevel());
      var skillRequirementMet = IsSkillRequirementMet(skill);

      // For debugging:
      // Helper.Log($"[PlayerSkillsManager] CanUnlock '{skill.Name}' ({skill.Version}) = {(levelRequirementMet & skillRequirementMet).ToString().ToUpper()} (Level req. is {levelRequirementMet.ToString().ToUpper()} & skill req. is {skillRequirementMet.ToString().ToUpper()}).");

      if (levelRequirementMet & skillRequirementMet)
        return true;
      return false;
    }

    private bool IsSkillRequirementMet(Skill skill)
    {
      if (skill.Requirements.SkillRequirement == null)
        return true;
      else
        return IsSkillUnlocked(skill.Requirements.SkillRequirement);
    }

    public bool TryUnlockAndActivateSkill(Skill skill)
    {
      var isUnlocked = IsSkillUnlocked(skill);
      if (CanUnlock(skill) & (isUnlocked == false))
      {
        if (skillPoints > 0)
        {
          unlockedSkills.Add(skill);
          activeSkills.Add(skill);
          RemoveDuplicatesInList(activeSkills, skill);
          EventManager.Instance.ChangeSkillPoints(-1);
          EventManager.Instance.UpdateActiveSkills();
          return true;
        }
        else
        {
          Helper.Log("[PlayerSkillsManager] Not enough skill points to unlock skill '" +
                     skill.Name + "'.");
          return false;
        }
      }

      if (isUnlocked)
        Helper.Log("[PlayerSkillsManager] Skill is already unlocked.");
      return false;
    }

    // Only used OnAwake() to ensure pre-configured skills are added to the active skills list.
    private bool TryActivateSkill(Skill skill)
    {
      // For debugging:
      // Helper.Log("[PlayerSkillsManager] Trying to activate skill '" + skill.Name + "' - is Unlocked: " + IsSkillUnlocked(skill) + ", is already active: " + activeSkills.Contains(skill) + ".");
      if (IsSkillUnlocked(skill) & (activeSkills.Contains(skill) == false))
      {
        activeSkills.Add(skill);
        return true;
      }

      return false;
    }

    private void RemoveDuplicatesInList(List<Skill> list, Skill skill)
    {
      list
        .FindAll(s =>
        {
          // For debugging:
          // Helper.Log("[PlayerSkillsManager] Comparing '" + s.Name + "' (V" + s.Version + ") with '" + 
          //     skill.Name + "' (V" + skill.Version + ") - target: " + s.Target.Equals(skill.Target) + 
          //     ", equipmentTarget: " + s.EquipmentTarget.Equals(skill) + " type: " + 
          //     s.Type.Equals(skill.Type) + ".");
          return s.Target.Equals(skill.Target) && s.Type.Equals(skill.Type) &&
                 s.EquipmentTarget.Equals(skill.EquipmentTarget);
        })
        .ForEach(s =>
          {
            if (skill.Version >= s.Version && skill.Id() != s.Id())
              // For debugging:
              // Helper.Log("[PlayerSkillsManager] Found '" + s.Name + "' (V" + s.Version + ") in active skills and replaced it with '" + skill.Name + "' (V" + skill.Version + ").");
              list.Remove(s);
          }
        );
    }

    #region Managing events

    public void ActionSkillPointChange(int change)
    {
      skillPoints += change;
    }

    private void OnEnable()
    {
      EventManager.Instance.OnSkillPointChange += ActionSkillPointChange;
      EventManager.Instance.OnSceneExit += TrySavePlayerPrefs;
    }

    private void OnDestroy()
    {
      EventManager.Instance.OnSkillPointChange -= ActionSkillPointChange;
      EventManager.Instance.OnSceneExit -= TrySavePlayerPrefs;
    }

    #endregion

    #region Loading from PlayerPrefs

    public void TryLoadPlayerPrefs()
    {
      skillPoints = PlayerPrefsManager.Instance.LoadInt(GlobalConstants.SKILL_POINTS);
      var namesOfUnlockedSkills =
        PlayerPrefsManager.Instance.LoadList(GlobalConstants.UNLOCKED_SKILLS);
      if (namesOfUnlockedSkills.Count > 0)
      {
        unlockedSkills.Clear();
        namesOfUnlockedSkills.ForEach(s =>
          unlockedSkills.Add(allSkills.Find(skill => skill.Name.Equals(s))));
        unlockedSkills.ForEach(s => TryActivateSkill(s));
        unlockedSkills.ForEach(s => RemoveDuplicatesInList(activeSkills, s));
      }

      // For debugging:
      // namesOfUnlockedSkills.ForEach(s => Helper.Log("[PlayerSkillsManager] Loaded skill '" + s + "'."));
    }

    public void TrySavePlayerPrefs(string spawnPointName)
    {
      PlayerPrefsManager.Instance.Save(GlobalConstants.SKILL_POINTS, skillPoints);
      var namesOfUnlockedSkills = new List<string>();
      unlockedSkills.ForEach(s => namesOfUnlockedSkills.Add(s.Name));
      PlayerPrefsManager.Instance.Save(GlobalConstants.UNLOCKED_SKILLS, namesOfUnlockedSkills);
    }

    #endregion
  }
}