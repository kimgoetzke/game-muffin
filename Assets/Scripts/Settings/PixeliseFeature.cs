using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CaptainHindsight.Settings
{
  public class PixeliseFeature : ScriptableRendererFeature
  {
    [System.Serializable]
    public class CustomPassSettings
    {
      public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
      public int screenHeight = 144;
    }

    [SerializeField] private CustomPassSettings settings;
    private PixeliseRenderPass _customPass;

    public override void Create()
    {
      _customPass = new PixeliseRenderPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer,
      ref RenderingData renderingData)
    {
#if UNITY_EDITOR
      if (renderingData.cameraData.isSceneViewCamera) return;
#endif

      renderer.EnqueuePass(_customPass);
    }
  }
}