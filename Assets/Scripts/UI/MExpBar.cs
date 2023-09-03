using CaptainHindsight.Core;
using CaptainHindsight.Managers;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaptainHindsight.UI
{
  public class MExpBar : MonoBehaviour
  {
    [SerializeField] private Slider slider;
    [ShowInInspector] private int _currentExperience;
    [ShowInInspector] private int _maxExperience = 10;
    [SerializeField] private TextMeshProUGUI textMesh;
    private int _currentLevel = 1;

    private void Awake()
    {
      EventManager.Instance.OnExperienceChange += SetExperience;
      EventManager.Instance.OnSkillPointChange += SetLevel;
      slider.maxValue = _maxExperience;
      slider.value = _currentExperience;
    }

    private void Start()
    {
      _currentLevel = Mathf.Max(PlayerPrefsManager.Instance.LoadInt(GlobalConstants.CURRENT_LEVEL),
        _currentLevel);
      textMesh.text = "LEVEL " + _currentLevel;
    }

    private void SetExperience(int experience, int maxExperience)
    {
      // For debugging:
      Helper.Log($"[MExpBar] Now at " + experience + " out of " + maxExperience + " experience.");
      slider.value = experience;
      slider.maxValue = maxExperience;
    }

    private void SetLevel(int change)
    {
      if (change <= 0) return;
      
      // For debugging:
      Helper.Log($"[MExpBar] Added " + change + " level.");
      _currentLevel += change;
      textMesh.text = "LEVEL " + _currentLevel;
    }

    private void OnDestroy()
    {
      EventManager.Instance.OnExperienceChange -= SetExperience;
      EventManager.Instance.OnSkillPointChange -= SetLevel;
    }
  }
}