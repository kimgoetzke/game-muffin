using UnityEngine;

namespace CaptainHindsight.Core
{
  public class MGizmoRayCameraToObject : MonoBehaviour
  {
    [SerializeField] private Color color = Color.red;
    private Camera _mainCamera;

    private void Awake()
    {
      _mainCamera = Camera.main;
    }

    private void OnDrawGizmos()
    {
      if (Application.isPlaying)
      {
        Gizmos.color = color;
        Gizmos.DrawLine(_mainCamera.transform.position, transform.position);
      }
    }
  }
}