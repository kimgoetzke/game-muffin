using CaptainHindsight.Core;
using CaptainHindsight.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Other
{
  public class ZSpawnPoint : MonoBehaviour
  {
    [ShowInInspector] [ReadOnly] private string _spawnPointName;
    [SerializeField] [Required] private GameObject enterTimeline;
    [SerializeField] [Required] private GameObject exitTimeline;

    private void Awake()
    {
      _spawnPointName = gameObject.name;
      PlayerPrefsManager.Instance.RegisterSceneChangePoint(_spawnPointName, enterTimeline);
    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.CompareTag("Player"))
      {
        Helper.Log("[ZSpawnPoint] Play timeline, then leave scene.");
        EventManager.Instance.ExitScene(_spawnPointName);
        exitTimeline.SetActive(true);
      }
    }

    private void OnValidate()
    {
      GetComponent<Collider>().isTrigger = true;
    }
  }
}