using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Data.Skills
{
    [CreateAssetMenu(fileName = "Skill-", menuName = "Scriptable Object/New skill")]
    public class Skill : ScriptableObject
    {

        [TitleGroup("Base settings")]
        public Sprite Icon;
        
        [Required]
        public string Name;

        [Required]
        public string Description;

        [ReadOnly]
        public int Id() 
        {
            return GetInstanceID();
        }

        public int Version;

        [TitleGroup("Requirements")]
        [Required]
        public SkillRequirements Requirements;

        [Serializable]
        public class SkillRequirements
        {
            public bool hasLevelRequirement;

            [ShowIf("hasLevelRequirement")]
            public int LevelRequirement;

            public bool LevelRequirementMet(int level)
            {
                if (hasLevelRequirement)
                {
                    if (level < LevelRequirement)
                        return false;
                }
                return true;
            }

            public bool hasSkillRequirement;

            [ShowIf("hasSkillRequirement")]
            public Skill SkillRequirement;
        }

        [TitleGroup("Categorisation")]

        [Required, SerializeField]
        public int Order;
        
        [Required]
        [SerializeField]
        public SkillTarget Target;

        public enum SkillTarget
        {
            Equipment,
            Player,
        }

        [ShowIf("@this.Target == SkillTarget.Equipment")]
        public Core.Equipment EquipmentTarget;

        public SkillType Type;

        public enum SkillType
        {
            HealthRegen,
            MaxHealth,
            Shield,
            Handling,
            FireRate,
            Projectiles
        }

        [TitleGroup("Skill settings")]
        [ShowIf("@this.Type == SkillType.FireRate " +
            "|| this.Type == SkillType.Projectiles " +
            "|| this.Type == SkillType.HealthRegen " +
            "|| this.Type == SkillType.MaxHealth")]
        public float Value;

        [TitleGroup("Skill settings")]
        [ShowIf("@this.Target == SkillTarget.Equipment && this.Type == SkillType.Handling")]
        public int EquipmentSlot;
    }
}
