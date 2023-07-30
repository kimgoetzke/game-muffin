using UnityEngine;

namespace CaptainHindsight.Other
{
  public class MDestroyOnAnimationEnd : MonoBehaviour
  {
    public void DestroyParentAfterAnimation()
    {
      var parent = gameObject.transform.parent.gameObject;
      Destroy(parent);
    }

    public void DestroyAfterAnimation()
    {
      Destroy(gameObject);
    }
  }
}