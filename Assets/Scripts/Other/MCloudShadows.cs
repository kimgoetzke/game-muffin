using DG.Tweening;
using UnityEngine;

namespace CaptainHindsight.Other
{
  public class MCloudShadows : MonoBehaviour
  {
    private Light _myLight;

    private void Start()
    {
      _myLight = GetComponent<Light>();
      _myLight.transform.DOMoveZ(25, 700).SetLoops(-1).SetEase(Ease.OutSine);
      _myLight.transform.DOMoveX(35, 1000).SetLoops(-1).SetEase(Ease.OutSine);
    }
  }
}