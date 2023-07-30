using CaptainHindsight.Core;
using TMPro;
using UnityEngine;

namespace CaptainHindsight.Other
{
  public class MEffectPopup : MonoBehaviour
  {
    [SerializeField] private TextMeshProUGUI textMesh;

    [SerializeField] private Color damageColour;
    [SerializeField] private Color healthColour;
    [SerializeField] private Color positiveColour;
    [SerializeField] private Color negativeColour;
    [SerializeField] private Color otherColour;

    private void Awake()
    {
      textMesh = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    public void Initialisation(ActionType effectType, string text)
    {
      textMesh.text = text;

      switch (effectType)
      {
        case ActionType.Damage:
          textMesh.color = damageColour;
          break;
        case ActionType.Health:
          textMesh.color = healthColour;
          break;
        case ActionType.Positive:
          textMesh.color = positiveColour;
          break;
        case ActionType.Negative:
          textMesh.color = negativeColour;
          break;
        default:
          textMesh.color = otherColour;
          break;
      }
    }
  }
}