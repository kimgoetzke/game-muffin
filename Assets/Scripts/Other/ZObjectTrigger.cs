using UnityEngine;

namespace CaptainHindsight.Other
{
  public class ZObjectTrigger : MonoBehaviour
  {
    [SerializeField] private GameObject[] objectsToTrigger;

    private void Start()
    {
      foreach (var obj in objectsToTrigger) obj.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
      foreach (var obj in objectsToTrigger) obj.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
      foreach (var obj in objectsToTrigger) obj.SetActive(false);
    }
  }
}