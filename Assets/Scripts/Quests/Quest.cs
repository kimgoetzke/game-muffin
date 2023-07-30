using System;
using System.Collections.Generic;
using CaptainHindsight.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using static CaptainHindsight.Quests.QuestData;

namespace CaptainHindsight.Quests
{
  [Serializable]
  public class Quest
  {
    [TitleGroup("Base settings")] [LabelWidth(105)] [PropertyOrder(-1)] [SerializeField]
    public string name;
    // {
    //   get => questData != null ? questData.questName : "Set quest data to see name...";
    //   set => questData.questName = value;
    // }

    [FormerlySerializedAs("QuestData")]
    [TitleGroup("Base settings")]
    [LabelWidth(105)]
    [Required]
    [AssetSelector(Paths = "Assets/Data/Quests")]
    public QuestData questData;

    [FormerlySerializedAs("State")]
    [LabelWidth(105)]
    [PropertySpace(SpaceAfter = 5, SpaceBefore = 0)]
    public QuestState state = QuestState.Unavailable;

    [FormerlySerializedAs("QuestRequirementStatus")]
    [TitleGroup("Requirements")]
    [ShowInInspector]
    [ReadOnly]
    [HideLabel]
    [LabelText("Fulfilled?")]
    public bool questRequirementStatus;

    public bool SetRequirementStatus(List<Quest> quests)
    {
      if (questData.questToComplete == null)
      {
        questRequirementStatus = true;
        return true;
      }

      var requiredQuest = quests.Find(q =>
        q.questData.questToComplete.questName == questData.questToComplete.questName);
      if (requiredQuest == null)
      {
        Helper.LogWarning("[Quest] Quest requirement: Quest was not found.");
        questRequirementStatus = true;
        return true;
      }
      else if (requiredQuest.state == QuestState.Completed)
      {
        questRequirementStatus = true;
        return true;
      }
      else
      {
        questRequirementStatus = false;
        return false;
      }
    }

    [FormerlySerializedAs("Tasks")] [TitleGroup("Details")]
    public List<QuestTask> tasks = new();

    [FormerlySerializedAs("QuestVariables")]
    [TitleGroup("Details")]
    [PropertySpace(SpaceAfter = 10, SpaceBefore = 5)]
    public List<QuestVariable> questVariables = new();

    public enum QuestState
    {
      Unavailable, // Set by QuestManager (SetAllQuestStates)
      Available, // Set by Quest (if questRequirements true) or QuestManager (SetAllQuestStates)
      Active, // Set by QuestManager through DialogueVariables
      Ready, // Set by QuestManager when all tasks are completed
      Completed // Set by QuestManager through DialogueVariables
    }

    #region Class: QuestTask

    [Serializable]
    public class QuestTask
    {
      [FormerlySerializedAs("QuestTaskData")]
      public QuestTaskData questTaskData;

      public TaskType Type => questTaskData.type;

      [ShowInInspector]
      public bool State
      {
        get
        {
          switch (questTaskData.type)
          {
            case TaskType.Do: return done;
            case TaskType.Defeat: return defeated >= questTaskData.toDefeat;
            case TaskType.Collect: return collected >= questTaskData.toCollect;
            case TaskType.Reach: return locationReached;
            default: return false;
          }
        }
      }

      [FormerlySerializedAs("Done")] [ShowIf("Type", TaskType.Do)]
      public bool done;

      [FormerlySerializedAs("Defeated")] [ShowIf("Type", TaskType.Defeat)] [MinValue(0)]
      public int defeated;

      [FormerlySerializedAs("Collected")] [ShowIf("Type", TaskType.Collect)]
      public int collected;

      [FormerlySerializedAs("LocationReached")] [ShowIf("Type", TaskType.Reach)] [SerializeField]
      private bool locationReached;

      public bool AtLocation(Transform reachedObject)
      {
        if (locationReached == true) return true;

        if ((reachedObject.position - questTaskData.location).sqrMagnitude < 2f)
        {
          Helper.Log("[Quest] Reach task: Location for a task was reached.");
          locationReached = true;
          return true;
        }
        else
        {
          return false;
        }
      }

      public void ResetTask()
      {
        done = false;
        defeated = 0;
        collected = 0;
        locationReached = false;
      }
    }

    #endregion


    #region Class: QuestVariable

    [Serializable]
    public class QuestVariable
    {
      [FormerlySerializedAs("QuestVariableData")]
      public QuestVariableData questVariableData;

      public VariableType Type => questVariableData.type;
      public bool SetByInk => questVariableData.setByInk;

      [FormerlySerializedAs("BoolValue")]
      [ShowIf("Type", VariableType.Bool)]
      [DisableIf("SetByInk")]
      [LabelText("Value")]
      public bool boolValue;

      [FormerlySerializedAs("IntValue")]
      [ShowIf("Type", VariableType.Int)]
      [DisableIf("SetByInk")]
      [LabelText("Value")]
      public int intValue;

      [FormerlySerializedAs("StringValue")]
      [ShowIf("Type", VariableType.String)]
      [DisableIf("SetByInk")]
      [LabelText("Value")]
      public string stringValue;

      public void ResetValues()
      {
        boolValue = false;
        intValue = 0;
        stringValue = "";
      }
    }

    #endregion

    public void UpdateQuestState()
    {
      var thereAreOutstandingTasksLeft = false;
      foreach (var task in tasks)
      {
        if (task.State == false) thereAreOutstandingTasksLeft = true;
      }

      if (thereAreOutstandingTasksLeft) return;

      // Set State to QuestState.Ready if there's a story (i.e. NPC) or
      // QuestState.Completed if there isn't
      state = questVariables.Count == 0 ? QuestState.Completed : QuestState.Ready;
    }


    public void Reset(List<Quest> quests)
    {
      tasks.ForEach(task => task.ResetTask());
      questVariables.ForEach(variable => variable.ResetValues());

      // This requires sequential sorting of the quest list because otherwise the
      // QuestRequirementStatus will be updated before the dependening quest
      // has been reset.
      state = SetRequirementStatus(quests) ? QuestState.Available : QuestState.Unavailable;
    }
  }
}