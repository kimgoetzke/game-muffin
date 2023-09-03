using CaptainHindsight.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Other
{
  public class ZSceneDefault : MonoBehaviour
  {
    [SerializeField] private bool isActive = true;

    [InfoBox(
      "If active, both of the below game objects will be deactivated on Awake() if the PlayerPrefsManager contains a spawn point.")]
    [SerializeField, Required]
    private GameObject enterTimeline;

    [SerializeField, Required] private GameObject[] timelineProps;

    private void Awake()
    {
      var isEnabled = PlayerPrefsManager.Instance.HasSpawnPoint() == false && isActive;
      enterTimeline.SetActive(isEnabled);
      foreach (var obj in timelineProps)
      {
        obj.SetActive(isEnabled);
      }
    }
  }
}