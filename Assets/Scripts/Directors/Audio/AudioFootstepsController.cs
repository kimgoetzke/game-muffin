using System;
using CaptainHindsight.Core;
using CaptainHindsight.Core.Observer;
using CaptainHindsight.Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CaptainHindsight.Directors.Audio
{
  public class AudioFootstepsController : BoolObserver
  {
    [SerializeField, Required] private LayerMask groundLayer;
    [ShowInInspector, ReadOnly] private bool _isInShallowWater;
    [SerializeField, Required] private TextureSound defaultTextureSound;
    [SerializeField, Required] private TextureSound shallowWaterTextureSound;
    [SerializeField, Required] private TextureSound[] textureSounds;
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    protected override void Awake()
    {
      base.Awake();
      RegisterObserverManually();
      defaultTextureSound.Initialise(gameObject);
      shallowWaterTextureSound.Initialise(gameObject);
      foreach (var t in textureSounds)
      {
        t.Initialise(gameObject);
      }
    }

    public override void ProcessInformation(bool isTrue)
    {
      _isInShallowWater = isTrue;
      // Helper.Log("[AudioFootstepsController] Is in shallow water: " + _isInShallowWater);
    }

    // Only used by animation events which are not recognised by IDE
    public void Play()
    {
      if (_isInShallowWater)
      {
        // Helper.Log("[AudioFootstepsController] Shallow Water.");
        shallowWaterTextureSound.Play();
        return;
      }

      if (Physics.Raycast(
            transform.position + new Vector3(0, 0.5f, 0),
            Vector3.down,
            out RaycastHit hit,
            1f,
            groundLayer) == false) return;

      if (hit.collider.TryGetComponent(out Terrain t))
      {
        PlaySoundFromTerrain(t, hit.point);
      }
      else if (hit.collider.TryGetComponent(out Renderer r))
      {
        PlaySoundFromRenderer(r);
      }
      else
      {
        Helper.LogWarning("[AudioFootstepsController] No terrain or renderer found.");
        defaultTextureSound.Play();
      }
    }

    private void PlaySoundFromTerrain(Terrain component, Vector3 hitInfoPoint)
    {
      var terrainPosition = hitInfoPoint - component.transform.position;
      var terrainData = component.terrainData;
      var splatMapPosition = new Vector3(
        terrainPosition.x / terrainData.size.x,
        0,
        terrainPosition.z / terrainData.size.z);
      var x = Mathf.FloorToInt(splatMapPosition.x * terrainData.alphamapWidth);
      var z = Mathf.FloorToInt(splatMapPosition.z * terrainData.alphamapHeight);
      var alphaMap = component.terrainData.GetAlphamaps(x, z, 1, 1);
      var primaryIndex = 0;
      for (var i = 1; i < alphaMap.Length; i++)
      {
        if (alphaMap[0, 0, i] > alphaMap[0, 0, primaryIndex])
        {
          primaryIndex = i;
        }
      }

      textureSounds[primaryIndex].Play();
    }

    private void PlaySoundFromRenderer(Renderer component)
    {
      var t = component.material.GetTexture(MainTex);
      foreach (var ts in textureSounds)
      {
        if (ts.texture.name != t.name) continue;
        ts.Play();
        return;
      }

      // Comment in for debugging render textures that play no sound:
      Helper.Log(
        $"[AudioFootstepsController] Unknown render texture: '{t}' on {component.gameObject.name}.");
    }
  }

  [Serializable]
  internal class TextureSound
  {
    [SerializeField, Required] public Texture texture;
    [SerializeField, Required] private Sfx sfx;
    [ShowInInspector, ReadOnly] private AudioSource _audioSource;

    public void Initialise(GameObject gameObject)
    {
      _audioSource =
        AudioDirector.Instance.InitialiseSfx(gameObject, sfx);
    }

    public string GetName()
    {
      return sfx.sfxClip.name;
    }

    public void Play()
    {
      _audioSource.Play();
      RandomisePitchAndVolume();
    }

    private void RandomisePitchAndVolume()
    {
      _audioSource.volume = sfx.sfxClip.volume +
                            Random.Range(-sfx.sfxClip.volumeVariation,
                              sfx.sfxClip.volumeVariation);
      _audioSource.pitch = sfx.sfxClip.pitch +
                           Random.Range(-sfx.sfxClip.pitchVariation, sfx.sfxClip.pitchVariation);
    }
  }
}