using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Other
{
  public class MOccludee : MonoBehaviour
  {
    [SerializeField] private Material transparencyMaterial;
    [SerializeField] private bool overrideTransparency;

    [SerializeField] [ShowIf("overrideTransparency")] [PropertyRange(0f, 1f)]
    private float overrideValue;

    private Material _defaultMaterial;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
      _spriteRenderer = GetComponent<SpriteRenderer>();
      _defaultMaterial = _spriteRenderer.material;
    }

    public void StopOccluding(bool status, float transparency)
    {
      if (status == false)
      {
        _spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        _spriteRenderer.material = _defaultMaterial;
      }
      else
      {
        if (overrideTransparency) transparency = overrideValue;
        _spriteRenderer.material = transparencyMaterial;
        _spriteRenderer.color = new Color(1f, 1f, 1f, transparency);
      }
    }
  }
}