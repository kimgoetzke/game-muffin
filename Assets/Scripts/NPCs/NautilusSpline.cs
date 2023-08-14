using CaptainHindsight.Directors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CaptainHindsight.NPCs
{
  public class NautilusSpline : MonoBehaviour
  {
    private Material _material;
    private static readonly int Colour = Shader.PropertyToID("_Colour");

    private void Awake()
    {
      _material = GetComponent<SpriteRenderer>().material;
    }

    private void Start()
    {
      var colours = ScriptableObjectsDirector.Instance.colours[0].Colours;
      var randomColour = Random.Range(0, colours.Length);
      _material.SetColor(Colour, colours[randomColour]);
    }
  }
}