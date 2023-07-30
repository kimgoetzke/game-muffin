using UnityEngine;

namespace CaptainHindsight.Other
{
  public class MSetGlobalShaderVariables : MonoBehaviour
  {
    [SerializeField] private RenderTexture renderTexture;

    private void Awake()
    {
      Shader.SetGlobalTexture("_GlobalWaterTexture", renderTexture);
    }
  }
}