using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHindsight.Core;
using CaptainHindsight.Data.Skills;
using CaptainHindsight.Directors;
using CaptainHindsight.Managers;
using CaptainHindsight.Player;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace CaptainHindsight.UI
{
  // This class implements BaseSubMenu which features the following methods:
  // - OpenMenu (abstract)
  // - BackToPreviousMenu (virtual)
  // - MoveButtonsOutOfView & MoveButtonsIntoView
  // - SetRaycaster
  // - OnEnable (which disables the raycaster)
  public class SkillsMenu : BaseSubMenu
  {
    [Title("General configuration")] [SerializeField, Required]
    public GameObject skillMenuUI;

    [SerializeField, Required] private GameObject skillTreeHolder;
    [SerializeField, Required] private GameObject gridElementPrefab;
    [ShowInInspector, ReadOnly] private IMenuNestable _parentMenu;
    private PlayerSkillsManager _playerSkillsManager;

    [Title("Skills settings")] [SerializeField, Required]
    private TextMeshProUGUI pointsMesh;

    [SerializeField] [ListDrawerSettings(ShowFoldout = true)] [ShowInInspector, ReadOnly]
    private List<Skill> allSkills = new();

    [ShowInInspector] [ReadOnly] private List<GameObject> _skillGridElements = new();

    private void Start()
    {
      _playerSkillsManager = PlayerSkillsManager.Instance;
      allSkills = _playerSkillsManager.GetAllSkills();
      allSkills.Sort((x, y) => x.Order.CompareTo(y.Order));
      pointsMesh.text = _playerSkillsManager.GetCurrentSkillPoints().ToString();
    }

    public override void OpenMenu(IMenuNestable origin)
    {
      _parentMenu = origin;
      SetRaycaster(true);
      SetSkills();
      skillMenuUI.SetActive(true);
      MoveButtonsIntoView();
      ShowSkills(0.3f);
    }

    private void SetSkills()
    {
      _skillGridElements.Clear();
      foreach (var skill in allSkills)
      {
        var gridElement = Instantiate(gridElementPrefab, skillTreeHolder.transform);
        gridElement.GetComponent<SkillMenuElement>().Initialise(skill);
        gridElement.SetActive(true);
        gridElement.transform.localScale = Vector3.zero;
        _skillGridElements.Add(gridElement);
      }
    }

    public override async void BackToPreviousMenu()
    {
      base.BackToPreviousMenu();
      PlayButtonPressSound();
      await HideSkills(0.1f);
      _parentMenu.ReturnFromSubMenu();
      skillMenuUI.SetActive(false);
    }

    private async void ShowSkills(float duration)
    {
      foreach (var element in _skillGridElements)
      {
        element.transform.DOScale(1f, duration).SetEase(Ease.OutBounce, 1f)
          .SetUpdate(UpdateType.Normal, true);
        await Task.Delay(System.TimeSpan.FromSeconds(duration / 6));
      }

      await Task.Delay(System.TimeSpan.FromSeconds(duration * 0F));
    }

    private async Task HideSkills(float duration)
    {
      foreach (var element in _skillGridElements)
      {
        element.transform.DOScale(new Vector3(0, 0, 0), duration).SetEase(Ease.InBounce)
          .SetUpdate(UpdateType.Normal, true);
        await Task.Delay(System.TimeSpan.FromSeconds(duration / 2));
      }

      await Task.Delay(System.TimeSpan.FromSeconds(duration));
      _skillGridElements.Clear();
      Helper.DeleteAllChildGameObjects(skillTreeHolder.transform);
    }

    #region Managing audio (Back button)

    private static void PlayButtonPressSound()
    {
      AudioDirector.Instance.Play("Click");
    }

    #endregion

    #region Managing events

    private void ActionSkillPointChange(int change)
    {
      var currentSkillPoints = _playerSkillsManager.GetCurrentSkillPoints();
      Helper.Log("[SkillMenu] Updating skill points to: " + currentSkillPoints + ".");
      pointsMesh.text = currentSkillPoints.ToString();
    }

    protected override void OnEnable()
    {
      base.OnEnable();
      EventManager.Instance.OnSkillPointChange += ActionSkillPointChange;
    }

    private void OnDestroy()
    {
      EventManager.Instance.OnSkillPointChange -= ActionSkillPointChange;
    }

    #endregion
  }
}