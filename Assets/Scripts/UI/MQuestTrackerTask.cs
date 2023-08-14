using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaptainHindsight.UI
{
  public class MQuestTrackerTask : MonoBehaviour
  {
    [Title("General")] [SerializeField] [Required]
    private Image indicator;

    [SerializeField] [Required] private TextMeshProUGUI textMesh;

    [Title("Complete settings")] [SerializeField] [Required] [LabelText("Indicator")]
    private Sprite taskCompleteIndicator;

    [SerializeField] [Required] [LabelText("Text colour")]
    private Color taskCompleteColour;

    [SerializeField] [Required] [LabelText("Text alpha")]
    private float taskIncompleteAlpha;

    [Title("Incomplete settings")] [SerializeField] [Required] [LabelText("Indicator")]
    private Sprite taskIncompleteIndicator;

    [SerializeField] [Required] [LabelText("Text colour")]
    private Color taskIncompleteColour;

    private bool _status;


    public void InitialiseTask(string text, bool complete)
    {
      UpdateName(text);
      _status = complete;
      UpdateStatus(_status);
    }

    public void UpdateStatus(bool complete)
    {
      _status = complete;
      if (_status)
      {
        textMesh.color = taskCompleteColour;
        textMesh.alpha = taskIncompleteAlpha;
        indicator.sprite = taskCompleteIndicator;
      }
      else
      {
        textMesh.color = taskIncompleteColour;
        textMesh.alpha = taskIncompleteAlpha;
        indicator.sprite = taskIncompleteIndicator;
      }
    }

    private void UpdateName(string text)
    {
      textMesh.text = text;
    }

    public bool GetStatus()
    {
      return _status;
    }
  }
}