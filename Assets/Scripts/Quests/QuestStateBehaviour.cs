using CaptainHindsight.Managers;
using UnityEngine;

namespace CaptainHindsight.Quests
{
    public abstract class QuestStateBehaviour : MonoBehaviour
    {
        protected abstract void ActionQuestUpdate(Quest quest, bool onlyStateChange);

        private void OnEnable()
        {
            EventManager.Instance.OnQuestStateChange += ActionQuestUpdate;
        }

        private void OnDisable()
        {
            EventManager.Instance.OnQuestStateChange -= ActionQuestUpdate;
        }
    }
}
