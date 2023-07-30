using CaptainHindsight.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Directors.Audio
{
  public class AudioLoop : MonoBehaviour
  {
    [SerializeField, Required] private Sfx sfx;

    [SerializeField]
    private int maxDistance = 6;
    private AudioSource _audioSource;

    private void Awake()
    {
      _audioSource =
        AudioDirector.Instance.InitialiseSfx(gameObject, sfx, false, maxDistance);
      _audioSource.loop = true;
      _audioSource.playOnAwake = true;
    }

    private void Start()
    {
      _audioSource.Play();
    }
  }
}