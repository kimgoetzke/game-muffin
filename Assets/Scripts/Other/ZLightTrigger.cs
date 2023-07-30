using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHindsight.Core;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CaptainHindsight.Other
{
  public class ZLightTrigger : MonoBehaviour
  {
    [Title("Settings")] [SerializeField] [ListDrawerSettings(ShowFoldout = true)]
    private List<LightSettings> lights = new();

    [Title("Time")]
    [SerializeField]
    [Tooltip("Time it takes to lerp to the new intensity value OnTriggerEnter")]
    [Min(0)]
    private float lerpIn;

    [SerializeField]
    [Tooltip("Time it takes to lerp back to the previous light intensity value OnTriggerExit")]
    [Min(0)]
    private float lerpOut;

    private void Awake()
    {
      for (var i = 0; i < lights.Count; i++)
      {
        lights[i].defaultIntensity = lights[i].light.intensity;
        lights[i].defaultTemperature = lights[i].light.colorTemperature;
        lights[i].defaultColour = lights[i].light.color;
      }
    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.gameObject.CompareTag("Player") == false) return;

      var killTweens = 0;
      for (var i = 0; i < lights.Count; i++)
      {
        killTweens += DOTween.Kill(lights[i].light);
        if (lights[i].changeIntensity) lights[i].light.DOIntensity(lights[i].intensity, lerpIn);
        if (lights[i].changeColour) lights[i].light.DOColor(lights[i].colour, lerpIn);
        if (lights[i].changeTemperature) ChangeColourTemperature(i, lights[i].temperature, lerpIn);
      }

      if (killTweens > 0)
        Helper.Log("[ZLightTrigger] Special light zone entered. Killed " + killTweens +
                   " tweens to avoid conflicts.");
    }

    private async void ChangeColourTemperature(int index, float temperature, float delay)
    {
      await Task.Delay(System.TimeSpan.FromSeconds(delay));

      lights[index].light.colorTemperature = temperature;
    }

    private void OnTriggerExit(Collider other)
    {
      if (other.gameObject.CompareTag("Player") == false) return;

      var killTweens = 0;
      for (var i = 0; i < lights.Count; i++)
      {
        killTweens += DOTween.Kill(lights[i].light);
        if (lights[i].changeIntensity)
          lights[i].light.DOIntensity(lights[i].defaultIntensity, lerpOut);
        if (lights[i].changeColour) lights[i].light.DOColor(lights[i].defaultColour, lerpOut);
        if (lights[i].changeTemperature)
          ChangeColourTemperature(i, lights[i].defaultTemperature, lerpOut);

        // TODO: Try to make this work (which may not be possible)!
        //DOTween.To(() => lights[i].Light.colorTemperature, x => lights[i].Light.colorTemperature = x, lights[i].DefaultTemperature, lerpIn);
      }

      if (killTweens > 0)
        Helper.Log("[ZLightTrigger] Special light zone left. Killed " + killTweens +
                   " tweens to avoid conflicts.");
    }

    [System.Serializable]
    private class LightSettings
    {
      [FormerlySerializedAs("Light")] [SerializeField] [SceneObjectsOnly] [HideLabel]
      public Light light;

      [FormerlySerializedAs("DefaultIntensity")] [HideInInspector]
      public float defaultIntensity;

      [FormerlySerializedAs("DefaultColour")] [HideInInspector]
      public Color defaultColour;

      [FormerlySerializedAs("DefaultTemperature")] [HideInInspector]
      public float defaultTemperature;

      [FormerlySerializedAs("ChangeIntensity")]
      [HorizontalGroup("Split1")]
      [VerticalGroup("Split1/Left")]
      [SerializeField]
      [LabelWidth(90)]
      [LabelText("Intensity")]
      public bool changeIntensity;

      [FormerlySerializedAs("Intensity")]
      [VerticalGroup("Split1/Right")]
      [SerializeField]
      [HideLabel]
      [ShowIf("ChangeIntensity")]
      [PropertyRange(0, 10)]
      public float intensity;

      [FormerlySerializedAs("ChangeColour")]
      [HorizontalGroup("Split2")]
      [VerticalGroup("Split2/Left")]
      [SerializeField]
      [LabelWidth(90)]
      [LabelText("Colour")]
      public bool changeColour;

      [FormerlySerializedAs("Colour")]
      [VerticalGroup("Split2/Right")]
      [SerializeField]
      [HideLabel]
      [ShowIf("ChangeColour")]
      public Color colour = Color.white;

      [FormerlySerializedAs("ChangeTemperature")]
      [HorizontalGroup("Split3")]
      [VerticalGroup("Split3/Left")]
      [SerializeField]
      [LabelWidth(90)]
      [LabelText("Temperature")]
      public bool changeTemperature;

      [FormerlySerializedAs("Temperature")]
      [VerticalGroup("Split3/Right")]
      [SerializeField]
      [HideLabel]
      [ShowIf("ChangeTemperature")]
      [PropertyRange(1500, 20000)]
      public float temperature = 1500;
    }
  }
}