using System;
using System.Collections.Generic;
using CaptainHindsight.Data.SFX;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Data.Equipment
{
    [CreateAssetMenu(fileName = "EquipmentItem-", menuName = "Scriptable Object/New equipment item")]
    public class EquipmentItem : ScriptableObject
    {
        [BoxGroup("Overview", ShowLabel = false)]
        [TitleGroup("Overview/Overview", horizontalLine: false)]
        [PreviewField(100, ObjectFieldAlignment.Left), HideLabel]
        [HorizontalGroup("Overview/Overview/Split", 105), PropertySpace(SpaceBefore = 5, SpaceAfter = 10)]
        public Sprite Icon;

        [VerticalGroup("Overview/Overview/Split/Right"), PropertySpace(SpaceBefore = 10)]
        [LabelWidth(80)]
        [Required]
        public Core.Equipment Type;

        [VerticalGroup("Overview/Overview/Split/Right"), PropertySpace(SpaceBefore = 4)]
        [LabelWidth(80)]
        [Required]
        public string Name;

        [VerticalGroup("Overview/Overview/Split/Right"), PropertySpace(SpaceBefore = 4)]
        [LabelWidth(80)]
        [Required]
        [AssetSelector(Paths = "Assets/Prefabs/")]
        public GameObject Prefab;

        [VerticalGroup("Overview/Overview/Split/Right"), PropertySpace(SpaceBefore = 4)]
        [LabelWidth(80)]
        [Required, MinValue(1)]
        public int Slot;

        [VerticalGroup("Overview/Overview/Split/Right"), PropertySpace(SpaceBefore = 4, SpaceAfter = 10)]
        [LabelWidth(80)]
        [Required, MinValue(0)]
        public int Cursor;

        [TitleGroup("Visuals")]
        [BoxGroup("Visuals/Sprites"), HideLabel]
        [Required]
        public DirectionalSprites Sprites;

        [Serializable]
        public class DirectionalSprites
        {
            [AssetSelector(Paths = "Assets/Sprites/Weapons/", DropdownWidth = 400, DrawDropdownForListElements = true)][HideLabel][PreviewField(60, ObjectFieldAlignment.Center)][HorizontalGroup("Row")] 
            public Sprite North;
            [AssetSelector(Paths = "Assets/Sprites/Weapons/", DropdownWidth = 400, DrawDropdownForListElements = true)][HideLabel][PreviewField(60, ObjectFieldAlignment.Center)][HorizontalGroup("Row")] 
            public Sprite West;
            [AssetSelector(Paths = "Assets/Sprites/Weapons/", DropdownWidth = 400, DrawDropdownForListElements = true)][HideLabel][PreviewField(60, ObjectFieldAlignment.Center)][HorizontalGroup("Row")] 
            public Sprite South;
            [AssetSelector(Paths = "Assets/Sprites/Weapons/", DropdownWidth = 400, DrawDropdownForListElements = true)][HideLabel][PreviewField(60, ObjectFieldAlignment.Center)][HorizontalGroup("Row")] 
            public Sprite East;
        };

        [Title("Sounds")]
        [LabelWidth(100), PropertySpace(SpaceBefore = 0, SpaceAfter = 10)]
        public List<SfxClip> AttackSounds;

        [Title("Stats")]
        [LabelWidth(100)]
        [MinValue(0f), PropertySpace(SpaceBefore = 0, SpaceAfter = 5)] public int DamageBase;

        [LabelWidth(100)]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 5)] public bool IsAutomatic;

        [LabelWidth(100)]
        [HideIf("IsAutomatic")]
        [MinValue(0f), PropertySpace(SpaceBefore = 0, SpaceAfter = 5)] public int ProjectileCount;

        [LabelWidth(100)]
        [HideIf("IsAutomatic")]
        [MinValue(0f), PropertySpace(SpaceBefore = 0, SpaceAfter = 5)] public int ProjectileSpread;

        [LabelWidth(100)]
        [MinValue(0f), PropertySpace(SpaceBefore = 0, SpaceAfter = 5)] public float AttackCooldown;
        
        [LabelWidth(100)]
        [MinValue(0f), MaxValue(2f), PropertySpace(SpaceBefore = 0, SpaceAfter = 5)] public float WalkSpeedModifier = 1;

        [LabelWidth(100)]
        [InlineButton("RecoilIntensity_XS", "XS")]
        [InlineButton("RecoilIntensity_S", "S")]
        [InlineButton("RecoilIntensity_M", "M")]
        [InlineButton("RecoilIntensity_L", "L")]
        [InlineButton("RecoilIntensity_XL", "XL")]
        [InlineButton("RecoilIntensity_XXL", "XXL"), PropertySpace(SpaceBefore = 0, SpaceAfter = 0)]
        [MinValue(0f)] public float RecoilIntensity;

        private void RecoilIntensity_XS() { RecoilIntensity = 0.5f; }
        private void RecoilIntensity_S() { RecoilIntensity = 0.75f; }
        private void RecoilIntensity_M() { RecoilIntensity = 1.5f; }
        private void RecoilIntensity_L() { RecoilIntensity = 2.5f; }
        private void RecoilIntensity_XL() { RecoilIntensity = 3.5f; }
        private void RecoilIntensity_XXL() { RecoilIntensity = 5f; }

        [LabelWidth(100), PropertySpace(SpaceBefore = 4, SpaceAfter = 10)]
        [InlineButton("RecoilLength_XS", "XS")]
        [InlineButton("RecoilLength_S", "S")]
        [InlineButton("RecoilLength_M", "M")]
        [InlineButton("RecoilLength_L", "L")]
        [InlineButton("RecoilLength_XL", "XL")]
        [InlineButton("RecoilLength_XXL", "XXL")]
        [MinValue(0f)] public float RecoilTime;

        private void RecoilLength_XS() { RecoilTime = 0.1f; }
        private void RecoilLength_S() { RecoilTime = 0.15f; }
        private void RecoilLength_M() { RecoilTime = 0.25f; }
        private void RecoilLength_L() { RecoilTime = 0.5f; }
        private void RecoilLength_XL() { RecoilTime = 1f; }
        private void RecoilLength_XXL() { RecoilTime = 1.5f; }

        [TitleGroup("Offsets")]

        [BoxGroup("Offsets/Left hand targets")]
        [LabelWidth(70)]
        [LabelText("North")] public Vector3 LHandTargetN;

        [BoxGroup("Offsets/Left hand targets")]
        [LabelWidth(70)]
        [LabelText("West")] public Vector3 LHandTargetW;

        [BoxGroup("Offsets/Left hand targets")]
        [LabelWidth(70)]
        [LabelText("South")] public Vector3 LHandTargetS;

        [BoxGroup("Offsets/Left hand targets")]
        [LabelWidth(70)]
        [LabelText("East")] public Vector3 LHandTargetE;

        [BoxGroup("Offsets/Right hand targets")]
        [LabelWidth(70)]
        [LabelText("North")] public Vector3 RHandTargetN;

        [BoxGroup("Offsets/Right hand targets")]
        [LabelWidth(70)]
        [LabelText("West")] public Vector3 RHandTargetW;

        [BoxGroup("Offsets/Right hand targets")]
        [LabelWidth(70)]
        [LabelText("South")] public Vector3 RHandTargetS;

        [BoxGroup("Offsets/Right hand targets")]
        [LabelWidth(70)]
        [LabelText("East")] public Vector3 RHandTargetE;
    }
}