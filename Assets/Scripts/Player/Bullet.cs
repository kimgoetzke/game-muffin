using System.Threading.Tasks;
using CaptainHindsight.Core;
using CaptainHindsight.Managers;
using CaptainHindsight.Other;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CaptainHindsight.Player
{
  public class Bullet : MonoBehaviour
  {
    [Title("Base settings")] [SerializeField]
    private float bulletSpeed = 1f;

    [ShowInInlineEditors] private int _bulletDamage;
    [SerializeField] private float deactivateAfter = 2f;
    [SerializeField, Required] private TrailRenderer trailRenderer;
    private Rigidbody _rb;

    [ShowInInspector] [ReadOnly]
    public Transform owner;

    [ShowInInspector, ReadOnly] private Quaternion _fixedRotation = Quaternion.LookRotation(Vector3.up);

    private void Awake()
    {
      _rb = GetComponent<Rigidbody>();
    }

    public void Initialise(Transform bulletOwner, Vector3 shootingDirection, bool trailOn,
      int damage)
    {
      owner = bulletOwner;
      _bulletDamage = damage;
      _rb.AddForce(shootingDirection * bulletSpeed);
      if (trailOn == false) trailRenderer.enabled = false;
      StartTimerToDeactivate();
    }

    private void OnCollisionEnter(Collision trigger)
    {
      if (trigger.transform.CompareTag("Bullet"))
        // For debugging:
        // Helper.Log("[Bullet] Collision protection: Collision with " + trigger.transform.name + " ignored.");
        return;

      // For debugging:
      // Helper.Log("[Bullet] Collision with " + trigger.transform.name + ".");
      if (trigger.transform.CompareTag("NPC") || 
          trigger.transform.CompareTag("Damageable") || 
          trigger.transform.CompareTag("Player"))
      {
        trigger.transform.GetComponent<IDamageable>().TakeDamage(_bulletDamage, owner);
      }

      var tagName = trigger.transform.tag;
      switch (tagName)
      {
        case "WaterRegular":
          ObjectPoolManager.Instance.SpawnFromPool("ammo-Impact-Water", transform.position,
            _fixedRotation);
          break;
        case "WaterSwamp":
          ObjectPoolManager.Instance.SpawnFromPool("ammo-Impact-Swamp", transform.position,
            _fixedRotation);
          break;
        case "NPC":
        case "Player":
          ObjectPoolManager.Instance.SpawnFromPool("ammo-Impact-Flesh", transform.position,
            Quaternion.identity);
          break;
        default:
          ObjectPoolManager.Instance.SpawnFromPool("ammo-Impact-Soil", transform.position,
            Quaternion.identity);
          break;
      }

      ResetBullet();
    }

    private async void StartTimerToDeactivate()
    {
      try
      {
        await Task.Delay(System.TimeSpan.FromSeconds(deactivateAfter));
        ResetBullet();
      }
      catch
      {
        // Deactivation will fail if the object has been destroyed in the meantime. 
        // This is ignored though.
      }
    }

    private void ResetBullet()
    {
      trailRenderer.enabled = true;
      var o = gameObject;
      o.SetActive(false);
      o.transform.position = new Vector3(0f, 0f, 0f);
      o.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
      _rb.velocity = new Vector3(0f, 0f, 0f);
    }
  }
}