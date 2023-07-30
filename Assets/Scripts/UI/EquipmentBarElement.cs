using CaptainHindsight.Data.Equipment;
using CaptainHindsight.Data.Skills;
using CaptainHindsight.Managers;
using CaptainHindsight.Player;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CaptainHindsight.UI
{
  public class EquipmentBarElement : MonoBehaviour
  {
    [TitleGroup("References")] [SerializeField] [Required]
    private GameObject holder;

    [SerializeField] [Required] private Sprite borderImageActive;
    [SerializeField] [Required] private Sprite borderImageInactive;
    [SerializeField] [Required] private Image iconImage;
    [SerializeField] [Required] private Image backgroundImage;

    [FormerlySerializedAs("SlotMesh")] [SerializeField] [Required]
    private TextMeshProUGUI slotMesh;

    [TitleGroup("Element settings")] [ShowInInspector] [ReadOnly]
    private EquipmentItem _equipmentData;

    [ShowInInspector] [ReadOnly] private int _slot;
    [ShowInInspector] [ReadOnly] private bool _isAvailable;
    [ShowInInspector] [ReadOnly] private bool _isActive;
    [ShowInInspector] [ReadOnly] private PlayerSkillsManager _playerSkillsManagger;

    private void Awake()
    {
      EventManager.Instance.OnEquipmentChange += UpdateIfEquipped;
      EventManager.Instance.OnActiveSkillsChange += UpdateElementIfAvailable;
    }

    public void Initialise(EquipmentItem equipment, PlayerSkillsManager playerSkillsManagger)
    {
      _equipmentData = equipment;
      iconImage.sprite = equipment.Icon;
      _slot = equipment.Slot;
      slotMesh.text = _slot.ToString();
      _playerSkillsManagger = playerSkillsManagger;
      UpdateElementIfAvailable(true);
    }

    private void UpdateIfEquipped(int equipmentSlot)
    {
      if (equipmentSlot == _slot)
      {
        backgroundImage.sprite = borderImageActive;
        gameObject.transform.DOKill(true);
        gameObject.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 10, 1);
      }
      else
      {
        backgroundImage.sprite = borderImageInactive;
      }
    }

    private void UpdateElementIfAvailable()
    {
      UpdateElementIfAvailable(false);
    }

    private void UpdateElementIfAvailable(bool isInitialising)
    {
      if (_isAvailable) return;

      // The 'no equipment' slot is always available and active by default
      if (_slot == 1)
      {
        _isAvailable = true;
        holder.SetActive(true);
        backgroundImage.sprite = borderImageActive;
        return;
      }

      if (_playerSkillsManagger
          .GetActiveSkills(Skill.SkillType.Handling)
          .Find(s => s.EquipmentSlot == _slot))
      {
        holder.SetActive(true);
        _isAvailable = true;
        if (isInitialising == false)
          gameObject.transform.DOPunchScale(Vector3.one * 0.7f, 0.2f, 10, 1);
      }
      else
      {
        holder.SetActive(false);
        _isAvailable = false;
      }
    }

    private void OnDestroy()
    {
      EventManager.Instance.OnEquipmentChange -= UpdateIfEquipped;
      EventManager.Instance.OnActiveSkillsChange -= UpdateElementIfAvailable;
    }
  }
}