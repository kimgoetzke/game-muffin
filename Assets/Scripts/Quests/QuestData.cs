using System.Collections.Generic;
using CaptainHindsight.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CaptainHindsight.Quests
{
  [CreateAssetMenu(fileName = "Quest-", menuName = "Scriptable Object/New quest")]
  public class QuestData : ScriptableObject
  {
    [FormerlySerializedAs("Name")] [Title("Settings")] [LabelWidth(90)]
    public string questName;

    [FormerlySerializedAs("Description")]
    [Title("Description", bold: false, horizontalLine: false)]
    [HideLabel]
    [MultiLineProperty]
    public string description;

    [TitleGroup("Requirements")]
    [HorizontalGroup("Requirements/Split")]
    [VerticalGroup("Requirements/Split/Left")]
    [SerializeField]
    [HideLabel]
    [LabelText("Quest")]
    [LabelWidth(90)]
    private bool questRequirement;

    [FormerlySerializedAs("QuestToComplete")]
    [VerticalGroup("Requirements/Split/Right")]
    [SerializeField]
    [HideLabel]
    [ShowIf("questRequirement")]
    public QuestData questToComplete;

    [FormerlySerializedAs("Story")]
    [Space]
    [Title("Story")]
    [LabelWidth(90)]
    [LabelText("Story JSON")]
    [AssetSelector(Paths = "Assets/Data/Stories")]
    public TextAsset story;

    [FormerlySerializedAs("QuestVariables")] [Space] [TitleGroup("Variables")]
    public List<QuestVariableData> questVariables = new();

    [FormerlySerializedAs("Tasks")] [Space] [Title("Tasks")] [ListDrawerSettings(ShowFoldout = true)]
    public List<QuestTaskData> tasks = new();

    [System.Serializable]
    public class QuestTaskData
    {
      [FormerlySerializedAs("Name")] public string name;

      [FormerlySerializedAs("Type")] public TaskType type;

      [FormerlySerializedAs("Description")] [MultiLineProperty] [LabelText("Info")] [LabelWidth(63)]
      public string description;

      [FormerlySerializedAs("ToDefeat")] [ShowIf("type", TaskType.Defeat)] [MinValue(1)]
      public int toDefeat;

      [FormerlySerializedAs("NPCIdentifier")]
      [LabelText("Identifier")]
      [ShowIf("type", TaskType.Defeat)]
      public NpcIdentifier npcIdentifier;

      [FormerlySerializedAs("ToCollect")] [ShowIf("type", TaskType.Collect)]
      public int toCollect;

      [FormerlySerializedAs("TypeToCollect")] [ShowIf("type", TaskType.Collect)]
      public int typeToCollect;

      [FormerlySerializedAs("Location")] [ShowIf("type", TaskType.Reach)]
      public Vector3 location;
    }


    [System.Serializable]
    public class QuestVariableData
    {
      [FormerlySerializedAs("Name")] public string name;
      [FormerlySerializedAs("Category")] public VariableCategory category;
      [FormerlySerializedAs("SetByInk")] public bool setByInk;

      [FormerlySerializedAs("Type")] [EnumToggleButtons]
      public VariableType type;
    }


    public enum TaskType
    {
      Do,
      Defeat,
      Collect,
      Reach
    }

    public enum VariableCategory
    {
      Unspecified, // Set by QuestManager
      QuestAccepted, // Set by DialogueVariable
      QuestReady, // Set by QuestManager
      QuestComplete, // Set by DialogueVariable
      KnowsNpc // Set by DialogueVariable
    }

    public enum VariableType
    {
      String,
      Bool,
      Int
    }
  }
}