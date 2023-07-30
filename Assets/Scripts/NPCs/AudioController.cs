using System.Collections.Generic;
using CaptainHindsight.Core;
using CaptainHindsight.Directors;
using CaptainHindsight.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.NPCs
{
  public class AudioController : MonoBehaviour
  {
    [SerializeField, Required] private Sfx[] attackSfx;
    [ShowInInspector, ReadOnly] private List<AudioSource> _attack = new();

    [SerializeField, Required] private Sfx deathSfx;
    [ShowInInspector, ReadOnly] private AudioSource _death;

    [SerializeField, Required] private Sfx hitSfx;
    [ShowInInspector, ReadOnly] private AudioSource _hit;

    [SerializeField, Required] private Sfx cooperateSfx;
    [ShowInInspector, ReadOnly] private AudioSource _cooperate;

    private void Awake()
    {
      foreach (var sfx in attackSfx)
      {
        _attack.Add(AudioDirector.Instance.InitialiseSfx(gameObject, sfx));
      }

      _death = AudioDirector.Instance.InitialiseSfx(gameObject, deathSfx);
      _hit = AudioDirector.Instance.InitialiseSfx(gameObject, hitSfx);
      _cooperate = AudioDirector.Instance.InitialiseSfx(gameObject, cooperateSfx);
    }

    internal void Play(AudioType audioType)
    {
      switch (audioType)
      {
        case AudioType.Attack:
          _attack[Random.Range(0, attackSfx.Length)].Play();
          break;
        case AudioType.Death:
          _death.Play();
          break;
        case AudioType.Cooperate:
          _cooperate.Play();
          break;
        case AudioType.Hit:
          _cooperate.Play();
          break;
        default:
          Helper.LogWarning($"[AudioController] AudioType '{audioType}' does not exist.");
          break;
      }
    }

    public enum AudioType
    {
      Attack,
      Death,
      Cooperate,
      Hit
    }
  }
}