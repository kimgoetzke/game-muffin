using System.Threading.Tasks;
using CaptainHindsight.Core.Observer;
using CaptainHindsight.Core.StateMachine;
using CaptainHindsight.Directors;
using CaptainHindsight.Managers;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace CaptainHindsight.UI
{
   // This class is used to display a button in the game world that can be
   // interacted with by the player.
   //
   // 1. The button can be configured to observe an NPC in order to disable the button
   //    when the "Attack" state is entered
   // 2. The button can be initialised by another class (e.g. QuestGiver) in order
   //    to set the interaction radius and the name of the story in order to trigger
   //    ExitInteractionTriggerRadius() when the player leaves the interaction radius
   //
   // This button does not allow any interaction by mouse. Use MButton for that (which
   // can be attached to the same component as this class.

  [RequireComponent(typeof(SphereCollider))]
  public class MButtonInGame : NpcObserver
  {
    [Title("Button settings")] [SerializeField] [Required]
    private GameObject element;

    [SerializeField, Required] private Sprite defaultImage, confirmedImage;
    [ShowInInspector, ReadOnly] private bool _isBlocked;
    [ShowInInspector, ReadOnly] private bool _isVisible;
    [ShowInInspector, ReadOnly] private bool _isLinkedToQuestGiver;
    [ShowInInspector] [ReadOnly] private string _nameOfStory;

    private Image _image;

    // TODO: Allow for the button to be used on objects not directly reachable
    // - Add a serialised variable that changes the radius on Awake()
    // - Add OnDrawGizmos to visualise the variable in the Unity Inspector
    // (Otherwise the player has to be within a 1m radius to interact)

    protected override void Start()
    {
      base.Start();
      _image = element.GetComponent<Image>();
      element.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.CompareTag("Player") == false || _isBlocked) return;

      _isVisible = true;
      element.SetActive(true);
      element.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 0.4f, 5);
      AudioDirector.Instance.Play("Select");
    }

    private async void OnTriggerExit(Collider other)
    {
      if (other.CompareTag("Player") == false || _isBlocked) return;

      _isVisible = false;
      if (_isLinkedToQuestGiver)
      {
        EventManager.Instance.ExitInteractionTriggerRadius(_nameOfStory);
      }

      await HideButton();
    }

    private async Task HideButton()
    {
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
      _isLinkedToQuestGiver = true;
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
      _isVisible = false;
    }

    public override void ProcessInformation(BState state)
    {
      switch (state.name)
      {
        case "Attack":
          _isBlocked = true;
#pragma warning disable CS4014
          if (_isVisible) HideButton();
#pragma warning restore CS4014
          break;
        default:
          _isBlocked = false;
          break;
      }
    }

    #endregion
  }
}