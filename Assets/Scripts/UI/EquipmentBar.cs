using System.Collections.Generic;
using CaptainHindsight.Data.Equipment;
using CaptainHindsight.Directors;
using CaptainHindsight.Managers;
using CaptainHindsight.Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.UI
{
  public class EquipmentBar : MonoBehaviour
  {
    [SerializeField] [Required] private GameObject equipmentElementPrefab;
    [SerializeField] [Required] private GameObject equipmentHolder;
    [ShowInInspector] [ReadOnly] private List<EquipmentItem> _equipmentList;
    private PlayerSkillsManager _playerSkillsManagger;
    [SerializeField] [Required] private Sprite borderImageActive;
    [SerializeField] [Required] private Sprite borderImageInactive;

    private void Awake()
    {
      EventManager.Instance.OnDialogueStateChange += ChangeVisibility;
    }

    private void Start()
    {
      if (IsInitialisationSuccessful() == false)
      {
        gameObject.SetActive(false);
        return;
      }

      AddComponentMenuGridElements();
    }

    private bool IsInitialisationSuccessful()
    {
      _playerSkillsManagger = PlayerSkillsManager.Instance;
      _equipmentList = ScriptableObjectsDirector.Instance.equipment;
      if (_playerSkillsManagger == null || _equipmentList == null || _equipmentList.Count == 0)
      {
        Debug.LogWarning(
          "[EquipmentBar] Requirements for displaying this component not fulfilled. It'll be hidden to prevent unexpected game behaviours.");
        return false;
      }

      return true;
    }

    private void AddComponentMenuGridElements()
    {
      _equipmentList.Sort((x, y) => x.Slot.CompareTo(y.Slot));
      for (var i = 0; i < _equipmentList.Count; i++)
      {
        var gridElement = Instantiate(equipmentElementPrefab, equipmentHolder.transform);
        gridElement.GetComponent<EquipmentBarElement>()
          .Initialise(_equipmentList[i], _playerSkillsManagger);
        gridElement.SetActive(true);
      }
    }

    private void ChangeVisibility(bool state)
    {
      gameObject.SetActive(!state);
    }

    private void OnDestroy()
    {
      EventManager.Instance.OnDialogueStateChange -= ChangeVisibility;
    }
  }
}