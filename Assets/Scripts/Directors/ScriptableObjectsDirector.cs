using System.Collections.Generic;
using CaptainHindsight.Data.Colours;
using CaptainHindsight.Data.Equipment;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CaptainHindsight.Directors
{
  public class ScriptableObjectsDirector : MonoBehaviour
  {
    public static ScriptableObjectsDirector Instance;

    [FormerlySerializedAs("Colours")] [AssetList(Path = "/Data/Colours/", AutoPopulate = true)]
    public ColourPalette[] colours;

    [FormerlySerializedAs("Equipment")] [AssetList(Path = "/Data/Equipment/", AutoPopulate = true)]
    public List<EquipmentItem> equipment;

    private void Awake()
    {
      if (Instance == null)
      {
        Instance = this;
        DontDestroyOnLoad(gameObject);
      }
      else
      {
        Destroy(gameObject);
        return;
      }
    }
  }
}