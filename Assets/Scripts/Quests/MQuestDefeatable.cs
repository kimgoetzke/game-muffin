using CaptainHindsight.Core;
using CaptainHindsight.Core.Observer;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Quests
{
  public class MQuestDefeatable : OriginObserver
  {
    [TitleGroup("Configuration")]
    [InfoBox(
      "This script can be placed anywhere but must be linked by setting the 'Observered Object'. It will automatically register itself and get notified when the NPC is defeated.")]
    [SerializeField]
    [Required]
    private NpcIdentifier identifier;

    public override void ProcessInformation(Transform origin)
    {
      if (origin.CompareTag("Player"))
        QuestManager.Instance.UpdateTask(QuestData.TaskType.Defeat, null, identifier);
    }
  }
}