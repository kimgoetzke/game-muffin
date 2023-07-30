using UnityEngine;

namespace CaptainHindsight.Other
{
  public interface IDamageable
  {
    public void TakeDamage(int damage, Transform origin = null);
  }
}