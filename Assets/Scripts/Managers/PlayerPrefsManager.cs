using System;
using System.Collections.Generic;
using System.Globalization;
using CaptainHindsight.Core;
using CaptainHindsight.Quests;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace CaptainHindsight.Managers
{
  public class PlayerPrefsManager : MonoBehaviour
  {
    public static PlayerPrefsManager Instance;

    [TitleGroup("Settings")] [SerializeField] [Required]
    private List<SpawnPoint> sceneChangePoints = new();

    private Transform _player;

    #region Managing Unity Editor-only buttons

    [TitleGroup("Editor actions")]
    [Button("Print PlayerPrefs for active scene", ButtonSizes.Large)]
    [GUIColor(0, 1, 0)]
    private void PrintAllPrefsViaEditorButton()
    {
      Helper.Log("[PlayerPrefsManager] Start of PlayerPrefs.");
      GlobalConstants.ForEachStringConstant(c =>
      {
        if (PlayerPrefs.HasKey(c.ToString()))
          Helper.Log("- " + c + "=" + PlayerPrefs.GetString(c.ToString()));
      });
      GlobalConstants.ForEachIntConstant(c =>
      {
        if (PlayerPrefs.HasKey(c.ToString()))
          Helper.Log("- " + c + "=" + PlayerPrefs.GetInt(c.ToString()));
      });
      GlobalConstants.ForEachFloatConstant(c =>
      {
        if (PlayerPrefs.HasKey(c.ToString()))
          Helper.Log("- " + c + "=" + PlayerPrefs.GetFloat(c.ToString()));
      });
      Helper.Log("[PlayerPrefsManager] End of PlayerPrefs.");
    }

    [HorizontalGroup("Editor actions/Split")]
    [VerticalGroup("Editor actions/Split/Left")]
    [Button("Delete spawn point", ButtonSizes.Large), PropertySpace(SpaceBefore = 5)]
    [GUIColor(1, 0, 0)]
    private void DeletePrefsForSceneViaEditorButton()
    {
      var sceneName = SceneManager.GetActiveScene().name;
      PlayerPrefs.DeleteKey(GlobalConstants.PREVIOUS_SCENE);
      PlayerPrefs.DeleteKey(GlobalConstants.PREVIOUS_SCENE_EXIT_SPAWN);
      Helper.Log("[PlayerPrefsManager] Deleted spawn point for scene '" + sceneName + "'.");
    }

    [VerticalGroup("Editor actions/Split/Right")]
    [Button("Delete all PlayerPrefs", ButtonSizes.Large), PropertySpace(SpaceBefore = 5)]
    [GUIColor(1, 0, 0)]
    private void DeleteAllPrefsViaEditorButton()
    {
      PlayerPrefs.DeleteAll();
      Helper.Log("[PlayerPrefsManager] Deleted all PlayerPrefs.");
    }

    #endregion

    private void Awake()
    {
      if (Instance == null)
        Instance = this;
      else
        Destroy(gameObject);
    }

    private void Start()
    {
      if (GameObject.Find("Player") == null)
      {
        Debug.LogWarning("[PlayerPrefsManager] Player transform not found.");
      }
      else
      {
        _player = GameObject.Find("Player").transform;
      }

      LoadSpawnPoint();
      var activeScene = SceneManager.GetActiveScene().name;
      if (activeScene.StartsWith("Level")) Save(GlobalConstants.ACTIVE_SCENE, activeScene);
    }

    #region Managing registration

    public void RegisterSceneChangePoint(string spawnPointName, GameObject timeline,
      Vector3 position)
    {
      var spawnPoint = new SpawnPoint
      {
        name = spawnPointName,
        position = position,
        timeline = timeline
      };
      sceneChangePoints.Add(spawnPoint);
      Helper.Log("[PlayerPrefsManager] Registered: " + spawnPoint.name + ".");
    }

    #endregion

    #region Managing loading

    public bool HasSpawnPoint()
    {
      return PlayerPrefs.HasKey(GlobalConstants.PREVIOUS_SCENE_EXIT_SPAWN);
    }

    private void LoadSpawnPoint()
    {
      if (PlayerPrefs.HasKey(GlobalConstants.PREVIOUS_SCENE_EXIT_SPAWN) == false) return;

      var previousSceneExit = PlayerPrefs.GetString(GlobalConstants.PREVIOUS_SCENE_EXIT_SPAWN);
      var spawnPoint = sceneChangePoints.Find(s => s.name == previousSceneExit);
      if (spawnPoint == null)
      {
        Helper.LogWarning("[PlayerPrefsManager] Failed to load: '" + previousSceneExit +
                          "' not found. Please review your PlayerPrefsManager for this scene " +
                          "and make sure it contains all relevant entrance/exit points.");
        return;
      }

      Helper.Log(
        $"[PlayerPrefsManager] Loaded: '{spawnPoint.name}' at {spawnPoint.position} for '{SceneManager.GetActiveScene().name}'.");
      _player.position = spawnPoint.position;
      spawnPoint.timeline.SetActive(true);
    }

    public List<Quest> LoadQuests()
    {
      if (PlayerPrefs.HasKey(GlobalConstants.ALL_QUESTS))
      {
        var json = PlayerPrefs.GetString(GlobalConstants.ALL_QUESTS);
        Helper.Log("[PlayerPrefsManager] Loaded: JSON = " + json + ".");
        var serialisedList = JsonUtility.FromJson<SerialisableQuestList>(json);
        var regularList = SerialisableQuestList.ToRegularList(serialisedList);
        LogLoadInfo(true, GlobalConstants.ALL_QUESTS, serialisedList.list.Count.ToString());
        return regularList;
      }

      LogLoadInfo(false, GlobalConstants.ALL_QUESTS);
      return new List<Quest>();
    }

    public List<string> LoadList(string key)
    {
      if (PlayerPrefs.HasKey(key))
      {
        var json = PlayerPrefs.GetString(key);
        var serialisedList = JsonUtility.FromJson<SerialisableStringList>(json);
        var regularList = SerialisableStringList.ToRegularList(serialisedList);
        LogLoadInfo(true, key, serialisedList.list.Count.ToString());
        return regularList;
      }

      LogLoadInfo(false, key);
      return new List<string>();
    }

    public string LoadString(string key)
    {
      if (PlayerPrefs.HasKey(key))
      {
        var value = PlayerPrefs.GetString(key);
        LogLoadInfo(true, key, value);
        return value;
      }

      LogLoadInfo(false, key);
      return "";
    }

    public int LoadInt(string key)
    {
      var value = 0;
      if (PlayerPrefs.HasKey(key))
      {
        value = PlayerPrefs.GetInt(key);
        LogLoadInfo(true, key, value.ToString());
      }
      else
      {
        LogLoadInfo(false, key);
      }

      return value;
    }

    public float LoadFloat(string key)
    {
      float value = 0;
      if (PlayerPrefs.HasKey(key))
      {
        value = PlayerPrefs.GetFloat(key);
        LogLoadInfo(true, key, value.ToString(CultureInfo.InvariantCulture));
      }
      else
      {
        LogLoadInfo(false, key);
      }

      return value;
    }

    #endregion

    #region Managing saving

    private void SaveSpawnPoint(string spawnPointName)
    {
      var sceneName = SceneManager.GetActiveScene().name;
      PlayerPrefs.SetString(GlobalConstants.PREVIOUS_SCENE, sceneName);
      PlayerPrefs.SetString(GlobalConstants.PREVIOUS_SCENE_EXIT_SPAWN, spawnPointName);
      // Helper.Log("[PlayerPrefsManager] Saved: Spawn point '" + spawnPointName + "' for '" +
      //            sceneName + "'.");
    }

    public void Save(string key, List<string> list)
    {
      // For debugging:
      // list.ForEach(i => Helper.Log("[PlayerPrefsManager] Received: " + i.ToString() + "."));

      var stringList = SerialisableStringList.FromRegularList(list);
      var json = JsonUtility.ToJson(stringList);
      PlayerPrefs.SetString(key, json);
      LogConversionInfo(json);
      LogSaveInfo(key, stringList.list.Count.ToString());
    }

    public void Save(List<Quest> quests)
    {
      // For debugging:
      // quests.ForEach(i => Helper.Log("[PlayerPrefsManager] Received: " + i.QuestData.Name + "."));

      var serialisableList = SerialisableQuestList.FromRegularList(quests);
      var json = JsonUtility.ToJson(serialisableList);
      PlayerPrefs.SetString(GlobalConstants.ALL_QUESTS, json);
      LogConversionInfo(json);
      LogSaveInfo(GlobalConstants.ALL_QUESTS, serialisableList.list.Count.ToString());
    }

    public void Save(string key, string value)
    {
      PlayerPrefs.SetString(key, value);
      LogSaveInfo(key, value);
    }

    public void Save(string key, int value)
    {
      PlayerPrefs.SetInt(key, value);
      LogSaveInfo(key, value.ToString());
    }

    public void Save(string key, float value)
    {
      PlayerPrefs.SetFloat(key, value);
      LogSaveInfo(key, value.ToString(CultureInfo.InvariantCulture));
    }

    #endregion

    #region Managing logging

    private void LogLoadInfo(bool status, string key, string value = null)
    {
      // if (status)
      // {
      //   if (value != null)
      //     Helper.Log("[PlayerPrefsManager] Loaded: " + key + " = " + value + ".");
      //   else Helper.Log("[PlayerPrefsManager] Loaded: " + key + ".");
      // }
      // else
      // {
      //   Helper.Log("[PlayerPrefsManager] Failed to load: " + key + ".");
      // }
    }

    private void LogConversionInfo(string json)
    {
      // Helper.Log("[PlayerPrefsManager] Converted list to: " + json);
    }

    private void LogSaveInfo(string key, string value = null)
    {
      // if (value == null)
      //   Helper.Log("[PlayerPrefsManager] Saved: " + key + ".");
      // else
      //   Helper.Log("[PlayerPrefsManager] Saved: " + key + " = " + value + ".");
    }

    #endregion

    #region Managing events

    private void OnEnable()
    {
      EventManager.Instance.OnSceneExit += SaveSpawnPoint;
    }

    private void OnDisable()
    {
      EventManager.Instance.OnSceneExit -= SaveSpawnPoint;
    }

    #endregion

    #region Managing serialisable classes

    [Serializable]
    private class SpawnPoint
    {
      public string name;
      public Vector3 position;
      public GameObject timeline;
    }

    [Serializable]
    private class SerialisableStringList
    {
      [FormerlySerializedAs("List")] public List<string> list;

      internal static SerialisableStringList FromRegularList(List<string> regularList)
      {
        return new SerialisableStringList { list = regularList };
      }

      internal static List<string> ToRegularList(SerialisableStringList serialisableList)
      {
        return new List<string>(serialisableList.list);
      }
    }

    [Serializable]
    private class SerialisableQuestList
    {
      [FormerlySerializedAs("List")] public List<Quest> list;

      internal static SerialisableQuestList FromRegularList(List<Quest> regularList)
      {
        return new SerialisableQuestList { list = regularList };
      }

      internal static List<Quest> ToRegularList(SerialisableQuestList serialisableList)
      {
        return new List<Quest>(serialisableList.list);
      }
    }

    // [Serializable]
    // private class SerialisableDictionary
    // {
    //     public List<ValuePair> List;

    //     public class ValuePair
    //     {
    //         public string Key;
    //         public string Value;
    //     }

    //     internal static SerialisableDictionary FromDictionary(Dictionary<string, string> regularDictionary)
    //     {
    //         List<ValuePair> serialisedDictionary = new List<ValuePair>();
    //         foreach (KeyValuePair<string, string> pair in regularDictionary)
    //         {
    //             serialisedDictionary.Add(new ValuePair
    //             {
    //                 Key = pair.Key,
    //                 Value = pair.Value
    //             });
    //         }
    //         return new SerialisableDictionary
    //         {
    //             List = serialisedDictionary
    //         };
    //     }

    //     internal static Dictionary<string, string> ToRegularList(SerialisableDictionary serialisableDictionary)
    //     {
    //         Dictionary<string, string> regularDictionary = new Dictionary<string, string>();
    //         foreach (ValuePair pair in serialisableDictionary.List)
    //         {
    //             regularDictionary.Add(pair.Key, pair.Value);
    //         }
    //         return regularDictionary;
    //     }
    // }

    #endregion
  }
}