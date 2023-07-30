using CaptainHindsight.Core;
using CaptainHindsight.Managers;
using UnityEngine;

namespace CaptainHindsight.Other
{
  public class MSceneTrigger : MonoBehaviour
  {
    [SerializeField] private string sceneName;

    private void OnTriggerEnter(Collider other)
    {
      if (other.CompareTag("Player"))
      {
        Helper.Log("[MSceneTrigger] Triggered scene: " + sceneName + ".");
        TransitionManager.Instance.FadeToNextScene(sceneName);
      }
    }
  }
}