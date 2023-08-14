using CaptainHindsight.Core;
using CaptainHindsight.Directors;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Quests
{
  [RequireComponent(typeof(BoxCollider))]
  public class MQuestLocation : QuestStateBehaviour
  {
    [SerializeField, Required] private Transform questLocation;
    [SerializeField, Required] private GameObject arrowParticles;
    [SerializeField, Required] private GameObject groundParticles;
    [SerializeField, Required] private GameObject successParticles;
    [SerializeField, Required] private BoxCollider boxCollider;

    private void Awake()
    {
      boxCollider.enabled = false;
      arrowParticles.SetActive(false);
      groundParticles.SetActive(false);
      successParticles.SetActive(false);
    }

    public void OnTriggerEnter(Collider other)
    {
      if (other.CompareTag("Player") == false) return;
      Helper.Log("[MQuestLocation] Quest location reached: " + transform.position + ".");
      QuestManager.Instance.UpdateTask(QuestData.TaskType.Reach, transform);
      AnimateDeactivation();
    }

    private void AnimateDeactivation()
    {
      AudioDirector.Instance.Play("Progression");
      arrowParticles.transform
        .DORotate(new Vector3(0, 180, 0), 0.5f)
        .onComplete += () =>
      {
        arrowParticles.SetActive(false);
        successParticles.SetActive(true);
        AudioDirector.Instance.Play("Boom");
      };
      groundParticles.transform
        .DOScale(0f, 0.5f)
        .onComplete += () => groundParticles.SetActive(false);
    }

    protected override void ActionQuestUpdate(Quest quest, bool onlyStateChange)
    {
      foreach (var task in quest.tasks)
      {
        if (task.Type != QuestData.TaskType.Reach) continue;
        if ((task.questTaskData.location - questLocation.position).sqrMagnitude > 1f) continue;
        if (quest.state != Quest.QuestState.Active) continue;

        Helper.Log(
          $"[MQuestLocation] Activated with distance to target: {(task.questTaskData.location - questLocation.position).sqrMagnitude}.");
        arrowParticles.SetActive(true);
        groundParticles.SetActive(true);
        boxCollider.enabled = true;
      }
    }
  }
}