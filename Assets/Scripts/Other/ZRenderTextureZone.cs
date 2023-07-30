using CaptainHindsight.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Other
{
  public class ZRenderTextureZone : MonoBehaviour
  {
    [SerializeField] private Camera renderCamera = null;

    [InfoBox(
      "There must be a corresponding 'Particles' child Game Object in the Player prefab that matches the name used here.")]
    [SerializeField]
    private string playerParticleName;

    private Transform _particleHolder;
    private Transform _particleTransform;

    private void Start()
    {
      renderCamera.enabled = false;

      // Find player
      if (GameObject.Find("Player") == null)
        Debug.LogError("[ZRenderTextureZone] Player transform not found.");
      else _particleHolder = GameObject.Find("Player").transform.Find("Particles").transform;
    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.CompareTag("Player") == false) return;
      Helper.Log("[ZRenderTextureZone] Entered render texture zone " + transform.parent.name + ".");
      renderCamera.enabled = true;
      _particleTransform = _particleHolder.Find(playerParticleName);
      _particleTransform.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
      if (other.CompareTag("Player") == false) return;
      renderCamera.enabled = false;
      _particleTransform.gameObject.SetActive(false);
      Helper.Log("[ZRenderTextureZone] Left render texture zone.");
    }

    private void OnValidate()
    {
      GetComponent<Collider>().isTrigger = true;
    }
  }
}