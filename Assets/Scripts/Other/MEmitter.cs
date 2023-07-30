using UnityEngine;

namespace CaptainHindsight.Other
{
  public class MEmitter : MonoBehaviour, INoticeable
  {
    [SerializeField] private float range = 1;
    [SerializeField] private LayerMask awarenessLayer;

    private void Start()
    {
      Emit();
    }

    public void Emit()
    {
      var colliders = Physics.OverlapSphere(transform.position, range, awarenessLayer);

      //Helper.Log("[MEmitter] Detected " + colliders.Length + " colliders.");
      for (var i = 0; i < colliders.Length; i++)
        //Helper.Log(" - " + (i + 1));
        colliders[i].GetComponent<INotice>().Notice(transform);
      //Helper.Log("[MEmitter] Notified " + colliders[i].transform.parent.name + " (" + colliders[i].name + ").");
    }
  }
}