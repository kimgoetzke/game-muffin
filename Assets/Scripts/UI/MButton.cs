using CaptainHindsight.Directors;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CaptainHindsight.UI
{
  public class MButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
  {
    [SerializeField] private Image image;
    [SerializeField] private Sprite defaultImage, pressedImage, confirmedImage;

    public void OnPointerEnter(PointerEventData eventData)
    {
      image.sprite = pressedImage;
      AudioDirector.Instance.Play("Select");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
      image.sprite = confirmedImage;
      AudioDirector.Instance.Play("Count");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
      image.sprite = confirmedImage;
      AudioDirector.Instance.Play("Click");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
      image.sprite = defaultImage;
      AudioDirector.Instance.Play("Type");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
      image.sprite = defaultImage;
    }
  }
}