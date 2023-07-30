using System.Collections.Generic;
using CaptainHindsight.Core;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CaptainHindsight.UI
{
  public class DialogueCanvasController : MonoBehaviour
  {
    [Title("General")] [Required] [SerializeField]
    private GameObject canvas;

    [FormerlySerializedAs("SpeakerMesh")] [Title("Text")] [Required]
    public TextMeshProUGUI speakerMesh;

    [FormerlySerializedAs("TextMesh")] [Required]
    public TextMeshProUGUI textMesh;

    [FormerlySerializedAs("Icon")] [Title("Icons")] [Required]
    public Image icon;

    [Required] [SerializeField] private Sprite continueIcon;
    [Required] [SerializeField] private Sprite endIcon;

    [Title("Buttons")] [Required] [SerializeField]
    private GameObject choicePrefab;

    [Required] [SerializeField] private GameObject choiceParent;

    public void SetCanvasActive(bool status)
    {
      canvas.SetActive(status);
    }

    public bool GetCanvasStatus()
    {
      return canvas.activeSelf;
    }

    public void ResetCanvas()
    {
      speakerMesh.text = "";
      textMesh.text = "...";
      Helper.DeleteAllChildGameObjects(choiceParent.transform);
      SetChoicesGroupActive(false);
    }

    public void SetChoicesGroupActive(bool status)
    {
      choiceParent.SetActive(status);
    }

    public void DisplayChoices(List<Ink.Runtime.Choice> currentChoices)
    {
      // Note that the layout group only supports 2 choices without overlapping with
      // the TextMesh - if you display 2 or less lines of text, 4 choices is possible

      // Delete children (i.e. buttons) of the choiceParent to reset the layout group
      Helper.DeleteAllChildGameObjects(choiceParent.transform);

      for (var i = 0; i < currentChoices.Count; i++)
      {
        var choiceTransform = Instantiate(choicePrefab, choiceParent.transform).transform;
        choiceTransform.GetComponentInChildren<TextMeshProUGUI>().text = currentChoices[i].text;
        choiceTransform.GetComponentInChildren<MSubmitChoice>().choiceIndex = i;
      }
    }
  }
}