using System.Collections.Generic;
using UnityEngine;

namespace CaptainHindsight.Other
{
  public class ZDamageTrigger : MonoBehaviour
  {
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float damageInterval = 1f;
    private bool _isCausingDamage;
    private readonly List<IDamageable> _damageables = new();
    private float _timer;

    private void OnTriggerEnter(Collider other)
    {
      if (other.CompareTag("Player") == false || other.CompareTag("NPC")) return;
      _isCausingDamage = true;
      _damageables.Add(other.GetComponent<IDamageable>());
    }

    private void OnTriggerExit(Collider other)
    {
      if (other.CompareTag("Player") == false || other.CompareTag("NPC")) return;
      _damageables.Remove(other.GetComponent<IDamageable>());
      if (_damageables.Count == 0) _isCausingDamage = false;
    }

    private void Update()
    {
      if (_isCausingDamage == false) return;
      _timer += Time.deltaTime;
      if (_timer >= damageInterval)
      {
        _timer = 0f;
        Damage();
      }
    }


    private void Damage()
    {
      foreach (var damageable in _damageables)
      {
        damageable.TakeDamage(damageAmount);
      }
    }
  }
}