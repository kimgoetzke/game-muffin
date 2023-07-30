using UnityEngine;

namespace CaptainHindsight.Core
{
  public class MGizmoSphereAtTransform : MonoBehaviour
  {
    [SerializeField] private float diameter = 0.1f;
    [SerializeField] private Color color = Color.red;

    private void OnDrawGizmos()
    {
      Gizmos.color = color;
      Gizmos.DrawSphere(transform.position, diameter);
    }
  }
}