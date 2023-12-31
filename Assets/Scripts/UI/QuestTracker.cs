using System.Collections.Generic;
using CaptainHindsight.Quests;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using static CaptainHindsight.Quests.Quest;

namespace CaptainHindsight.UI
{
  public class QuestTracker : QuestStateBehaviour
  {
    [Title("Configuration")] [SerializeField] [Required]
    private TextMeshProUGUI titleMesh;

    [SerializeField] [Required] private GameObject taskTemplatePrefab;
    [SerializeField] [Required] private GameObject questHolder;
    [ShowInInspector] [ReadOnly] private List<QuestTrackerTask> _mQuestTrackerTasks = new();

    protected override void ActionQuestUpdate(Quest quest, bool onlyStateChange)
    {
      // For debugging:
      // Helper.Log("[MQuestTracker] Updates to '" + quest.QuestData.Name + "' (onlyStateChange=" + onlyStateChange.ToString().ToUpper() + ") are being processed.");
      if (onlyStateChange)
      {
        ActionQuestStateChange(quest);
        return;
      }

      UpdateTasks(quest);
    }

    private void ActionQuestStateChange(Quest quest)
    {
      if (quest.state == QuestState.Completed)
      {
        ResetQuestTracker();
      }
      else
      {
        InitialiseQuestTracker(quest);
      }
    }

    private void ResetQuestTracker()
    {
      if (_mQuestTrackerTasks.Count > 0)
        _mQuestTrackerTasks.ForEach(task => Destroy(task.gameObject));

      titleMesh.text = "Undefined";
      questHolder.SetActive(false);
    }

    private void InitialiseQuestTracker(Quest quest)
    {
      questHolder.SetActive(true);
      titleMesh.text = quest.questData.questName;

      if (_mQuestTrackerTasks.Count > 0)
        _mQuestTrackerTasks.ForEach(task => Destroy(task.gameObject));

      foreach (var t in quest.tasks)
      {
        var taskHolder = Instantiate(taskTemplatePrefab, questHolder.transform, false);
        var task = taskHolder.GetComponent<QuestTrackerTask>();
        task.InitialiseTask(t.questTaskData.name, t.State);
        _mQuestTrackerTasks.Add(task);
      }

      SetToCompleteIfTasksCompleted();
    }

    private void UpdateTasks(Quest quest)
    {
      for (var i = 0; i < quest.tasks.Count; i++)
      {
        _mQuestTrackerTasks[i].UpdateStatus(quest.tasks[i].State);
      }

      SetToCompleteIfTasksCompleted();
    }

    private void SetToCompleteIfTasksCompleted()
    {
      var allComplete = true;
      foreach (var task in _mQuestTrackerTasks)
        if (task.GetStatus() == false)
          allComplete = false;

      if (allComplete) titleMesh.text += " [DONE]";
    }
  }
}