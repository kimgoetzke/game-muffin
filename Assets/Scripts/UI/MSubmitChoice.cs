using CaptainHindsight.Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace CaptainHindsight.UI
{
  public class MSubmitChoice : MonoBehaviour
  {
    [FormerlySerializedAs("ChoiceIndex")] public int choiceIndex;

    public void SubmitChoice()
    {
      EventManager.Instance.SubmitDialogueChoice(choiceIndex);
    }
  }
}