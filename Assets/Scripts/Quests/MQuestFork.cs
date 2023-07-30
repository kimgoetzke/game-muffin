using CaptainHindsight.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using static CaptainHindsight.Quests.Quest;

namespace CaptainHindsight.Quests
{
  public class MQuestFork : MonoBehaviour
  {
    [SerializeField] [Required] private QuestManager questManager;

    [Title("If")] [LabelText("Quest")] [SerializeField] [Required]
    private QuestData questData;

    [HideInInspector] [ReadOnly] private Quest _quest;

    [LabelText("Is")] [SerializeField] [Required]
    private bool isIn;

    [ShowIf("isIn")] [LabelText("In state")] [SerializeField] [Required]
    private QuestState questState;

    [LabelText("Is NOT")] [SerializeField] [Required]
    private bool isNotIn;

    [ShowIf("isNotIn")] [LabelText("In state")] [SerializeField] [Required]
    private QuestState notQuestState;

    [Title("Then")] [LabelText("Activate these objects")] [SerializeField] [Required]
    private GameObject[] objectsToActivate;

    [LabelText("And dectivate these objects")] [SerializeField] [Required]
    private GameObject[] objectsToDeactivate;

    private void Start()
    {
      _quest = questManager.GetQuest(questData);
      if (isIn == false && isNotIn == false)
        Helper.LogWarning(
          $"[MQuestFork] Quest {questData.name} has no condition and defaults to criteria NOT fulfilled.");

      if (_quest == null)
      {
        Helper.LogWarning($"[MQuestFork] Quest {questData.name} not found in QuestManager.");
        return;
      }

      if (isIn && _quest.state == questState)
      {
        CriteriaFulfilled(true);
        return;
      }
      else if (isNotIn && _quest.state != notQuestState)
      {
        CriteriaFulfilled(true);
        return;
      }

      CriteriaFulfilled(false);
    }

    private void CriteriaFulfilled(bool active)
    {
      foreach (var obj in objectsToDeactivate) obj.SetActive(!active);

      foreach (var obj in objectsToActivate) obj.SetActive(active);
    }
  }
}