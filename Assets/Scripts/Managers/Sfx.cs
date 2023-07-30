using System.Collections.Generic;
using CaptainHindsight.Data.SFX;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CaptainHindsight.Managers
{
  [System.Serializable]
  public class Sfx
  {
    #region Defining variables

    [LabelText("Category")] [LabelWidth(100)] [OnValueChanged("SfxChange")]
    public SfxCategory sfxCategory = SfxCategory.Player;

    [FormerlySerializedAs("sfxToPlay")]
    [LabelText("$_sfxLabel")]
    [LabelWidth(100)]
    [ValueDropdown("Category", DropdownWidth = 400)]
    [OnValueChanged("SfxChange")]
    [InlineButton("Preview")]
    [InlineButton("Select")]
    public SfxClip sfxClip;

    private string _sfxLabel = "SFX";
#pragma warning disable
    [SerializeField] private bool showFile = false;
#pragma warning enable

    [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    [ShowIf("showFile")]
    [SerializeField]
    private SfxClip sfxBase;

    #endregion

    #region Methods used in Unity Inspector

    private void SfxChange()
    {
      _sfxLabel = sfxCategory + " SFX";
      sfxBase = sfxClip;
    }

#if UNITY_EDITOR
    private void Select()
    {
      UnityEditor.Selection.activeObject = sfxClip;
    }
#endif

    private void Preview()
    {
      SfxManager.PlaySfx(sfxClip);
    }

    // Get list of SFX from AudioDirector, used in the Unity Inspector
    private List<SfxClip> Category()
    {
      var sfxList = sfxCategory switch
      {
        SfxCategory.Effects => SfxManager.Instance.effectSfx,
        SfxCategory.Equipment => SfxManager.Instance.equipmentSfx,
        SfxCategory.Npc => SfxManager.Instance.npcSfx,
        SfxCategory.Player => SfxManager.Instance.playerSfx,
        SfxCategory.Ambience => SfxManager.Instance.ambienceSfx,
        _ => SfxManager.Instance.playerSfx
      };

      return sfxList;
    }

    #endregion

    public void Play(AudioSource audioSource, bool waitToFinish = false)
    {
      SfxManager.PlaySfx(sfxClip, audioSource, waitToFinish);
    }

    public enum SfxCategory
    {
      Effects,
      Equipment,
      Npc,
      Player,
      Ambience
    }
  }
}