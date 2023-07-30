using CaptainHindsight.Core;
using UnityEngine;

namespace CaptainHindsight.Other
{
  public class MSpriteZOffset : MonoBehaviour
  {
    private SpriteRenderer[] _renderers;

    private void Start()
    {
      _renderers = GetComponentsInChildren<SpriteRenderer>(true);

      foreach (var item in _renderers)
      {
        var offset = item.sortingOrder / 100000f;
        var newZPosition = item.transform.localPosition.z + offset;
        Helper.Log("(" + item.transform.parent.name + ") " + item.transform.name + ", order: " +
                   item.sortingOrder + ", Z: " + item.transform.localPosition.z + " - Offset by " +
                   offset + " to " + newZPosition + ".");
        item.transform.position = new Vector3(item.transform.position.x, item.transform.position.y,
          newZPosition);
      }
    }
  }
}