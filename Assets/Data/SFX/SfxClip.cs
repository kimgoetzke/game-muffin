using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CaptainHindsight.Data.SFX
{
    [CreateAssetMenu(menuName = "Scriptable Object/New SFX clip", fileName = "SFX-")]
    public class SfxClip : ScriptableObject
    {
        [FormerlySerializedAs("Clip")]
        [Space]
        [Title("File")]
        [Required]
        public AudioClip clip;

        [FormerlySerializedAs("Volume")]
        [Title("Settings")]
        [Range(0f, 2f)]
        public float volume = 0.95f;

        [FormerlySerializedAs("VolumeVariation")] [Range(0f, 0.2f)]
        public float volumeVariation = 0.05f;

        [FormerlySerializedAs("Pitch")] [Range(0f, 2f)]
        public float pitch = 1f;

        [FormerlySerializedAs("PitchVariation")] [Range(0f, 0.2f)]
        public float pitchVariation = 0.05f;
    }
}