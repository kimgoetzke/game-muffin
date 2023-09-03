using CaptainHindsight.Managers;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace CaptainHindsight.UI
{
  // This class is used by sub menus such as the skills menu to abstract common behaviours
  // and ensure accessibility from base menus e.g. using IMenuNestable
  public abstract class BaseSubMenu : MonoBehaviour
  {
    [SerializeField] [ListDrawerSettings(ShowFoldout = true)] [ChildGameObjectsOnly]
    private RectTransform[] buttons;

    [SerializeField] private GraphicRaycaster raycaster;

    public abstract void OpenMenu(IMenuNestable origin);

    public virtual void BackToPreviousMenu()
    {
      SetRaycaster(false);
      MoveButtonsOutOfView();
    }

    protected void MoveButtonsOutOfView()
    {
      var sequence = DOTween.Sequence().SetUpdate(UpdateType.Normal, true);
      foreach (var button in buttons)
        sequence.Append(button.DOAnchorPosY(-60, 0.2f).SetEase(Ease.Linear));
    }

    protected void MoveButtonsIntoView()
    {
      var sequence = DOTween.Sequence().SetUpdate(UpdateType.Normal, true);
      foreach (var button in buttons)
        sequence.Append(button.DOAnchorPosY(25, 0.2f).SetEase(Ease.Linear));
    }

    // Raycasters block input to reach objects beneath them. Therefore, the raycaster 
    // must be disabled OnEnable and only be enabled when the menu is opened. Otherwise,
    // the menu will block interactions with other menus and possible a touch controller.
    protected void SetRaycaster(bool isEnabled)
    {
      raycaster.enabled = isEnabled;
    }

    protected virtual void OnEnable()
    {
      SetRaycaster(false);
    }
  }
}