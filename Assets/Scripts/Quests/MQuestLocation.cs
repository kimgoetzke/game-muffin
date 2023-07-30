using CaptainHindsight.Core;
using UnityEngine;

namespace CaptainHindsight.Quests
{
  [RequireComponent(typeof(BoxCollider))]
  public class MQuestLocation : MonoBehaviour
  {
    public void OnTriggerEnter(Collider other)
    {
      if (other.CompareTag("Player"))
      {
        Helper.Log("[MQuestLocation] Quest location reached: " + transform.position + ".");
        QuestManager.Instance.UpdateTask(QuestData.TaskType.Reach, transform);
      }
    }
  }
}