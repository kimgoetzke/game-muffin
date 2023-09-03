using CaptainHindsight.Directors.Audio;
using CaptainHindsight.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Other
{
  public class MDebris : MonoBehaviour
  {
    [SerializeField, Required] private GameObject waterSpray;
    [SerializeField, Required] private GameObject waterRipple;
    [SerializeField] private bool isImpactedByForce;
    [SerializeField, Required] private AudioOneShot audioOneShot;

    [SerializeField, ShowIf("isImpactedByForce")]
    private float forceToAdd = 10f;

    private readonly Quaternion _fixedRotation = Quaternion.LookRotation(Vector3.up);
    private Rigidbody _rb;
    private bool _hasCollided;

    private void Awake()
    {
      _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
      if (isImpactedByForce)
      {
        _rb.AddForce(transform.forward * forceToAdd);
      }
    }

    // An elegant and scalable way of doing this would be to create a debris particle system
    // which is attached to this Game Object and control emission rate over time based on 
    // the velocity of the debris. However, this cannot be justified for this one-off use case.
    private void OnCollisionEnter(Collision other)
    {
      if (_hasCollided) return;
      switch (other.transform.tag)
      {
        case "Metal":
          ObjectPoolManager.Instance.SpawnFromPool("ammo-Impact-Metal", transform.position,
            _fixedRotation);
          break;
        case "Stone":
          ObjectPoolManager.Instance.SpawnFromPool("ammo-Impact-Soil", transform.position,
            _fixedRotation);
          break;
        case "NPC":
        case "Player":
          ObjectPoolManager.Instance.SpawnFromPool("ammo-Impact-Flesh", transform.position,
            Quaternion.identity);
          break;
      }

      _hasCollided = true;
    }

    private void OnTriggerEnter(Collider other)
    {
      var tagName = other.transform.tag;
      // Helper.Log($"Debris collision with {other.gameObject.name} ({tagName}, layer {other.gameObject.layer}).");
      switch (tagName)
      {
        case "WaterRegular":
          audioOneShot.Play();
          waterSpray.SetActive(true);
          waterRipple.SetActive(true);
          _rb.velocity = Vector3.zero;
          break;
        case "WaterSwamp":
          audioOneShot.Play();
          waterSpray.SetActive(true);
          waterRipple.SetActive(true);
          _rb.velocity = Vector3.zero;
          break;
      }
    }
  }
}