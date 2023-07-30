using System.Threading.Tasks;
using CaptainHindsight.Directors;
using CaptainHindsight.Managers;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace CaptainHindsight.UI
{
  [RequireComponent(typeof(SphereCollider))]
  public class MButtonInGame : MonoBehaviour
  {
    [Title("Configuration")] [SerializeField] [Required]
    private GameObject element;

    [SerializeField] [Required] private Sprite defaultImage, pressedImage, confirmedImage;

    private Image _image;

    [ShowInInspector] [ReadOnly] private string _nameOfStory;

    // TODO: Allow for the button to be used on objects not directly reachable
    // - Add a serialised variable that changes the radius on Awake()
    // - Add OnDrawGizmos to visualise the variable in the Unity Inspector
    // (Otherwise the player has to be within a 1m radius to interact)

    private void Start()
    {
      _image = element.GetComponent<Image>();
      element.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.CompareTag("Player") == false) return;

      element.SetActive(true);
      element.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 0.4f, 5);
      AudioDirector.Instance.Play("Select");
    }

    private async void OnTriggerExit(Collider other)
    {
      if (other.CompareTag("Player") == false) return;

      EventManager.Instance.ExitInteractionTriggerRadius(_nameOfStory);
      element.transform.DOScale(0f, 0.35f);
      AudioDirector.Instance.Play("Type");
      await Task.Delay(System.TimeSpan.FromSeconds(0.35f));
      element.SetActive(false);
      ResetUI();
    }

    private void ResetUI()
    {
      _image.sprite = defaultImage;
      element.transform.DOScale(1f, 0f);
    }

    #region Public methods used by other scripts (e.g. QuestGiver)

    public void InitialiseButton(float interactionRadius, string storyName)
    {
      GetComponent<SphereCollider>().radius = interactionRadius;
      _nameOfStory = storyName;
    }

    public async void Interact()
    {
      element.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 0.3f, 5);
      _image.sprite = confirmedImage;
      AudioDirector.Instance.Play("Click");
      await Task.Delay(System.TimeSpan.FromSeconds(0.3f));
      element.transform.DOScale(0f, 0.35f);
      await Task.Delay(System.TimeSpan.FromSeconds(0.35f));
      element.SetActive(false);
      ResetUI();
    }

    #endregion
  }
}