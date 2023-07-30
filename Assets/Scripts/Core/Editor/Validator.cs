#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CaptainHindsight.Core.Editor
{
  public class Validator : UnityEditor.Editor
  {
    [MenuItem("Assets/[Validator] Check If Material Used")]
    private static void CheckMaterial()
    {
      var matToCheck = Selection.activeObject as Material;
      var found = false;

      // Check all mesh renderers
      foreach (var renderer in FindObjectsOfType<MeshRenderer>(true))
        if (renderer.sharedMaterials.Contains(matToCheck))
        {
          Helper.LogWarning(
            "[Validator] Asset detection: Material used by " + renderer.transform.name +
            " (mesh renderer).", renderer.gameObject);
          found = true;
        }

      // Check all sprite renderers
      foreach (var renderer in FindObjectsOfType<SpriteRenderer>(true))
        if (renderer.sharedMaterials.Contains(matToCheck))
        {
          Helper.LogWarning(
            "[Validator] Asset detection: Material used by " + renderer.transform.name +
            "(sprite renderer).", renderer.gameObject);
          found = true;
        }

      // Check all particle renderers
      foreach (var renderer in FindObjectsOfType<ParticleSystemRenderer>(true))
        if (renderer.sharedMaterials.Contains(matToCheck))
        {
          Helper.LogWarning(
            "[Validator] Asset detection: Material used by " + renderer.transform.name +
            " (particle system renderer).", renderer.gameObject);
          found = true;
        }

      // Confirm if not found
      if (found == false)
        Helper.Log(
          "[Validator] Asset detection: Material not found in any renderer in the current scene.");
    }

    //[MenuItem("Assets/[Validator] Check If Material Used", true)]
    //private static bool CheckMaterialValidation()
    //{
    //    return Selection.activeObject is Material;
    //}
  }
}
#endif