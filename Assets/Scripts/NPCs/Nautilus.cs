using System;
using System.Threading.Tasks;
using CaptainHindsight.Directors;
using CaptainHindsight.Directors.Audio;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CaptainHindsight.NPCs
{
  public class Nautilus : MonoBehaviour
  {
    [SerializeField] private Transform parentTransform;
    [SerializeField] private Transform[] waypoints;
    private TrailRenderer _trailRenderer;
    private SpriteRenderer _spriteRenderer;
    private Material _material;
    private Animator _animator;
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int Idle = Animator.StringToHash("idle");
    private static readonly int Colour = Shader.PropertyToID("_Colour");
    [SerializeField] private float mSpeed = 1;
    [ShowInInspector, ReadOnly] private bool _isMoving;
    [ShowInInspector, ReadOnly] private bool _isFacingRight;
    private int _currentPosition;
    private Vector3 _destination;
    private AudioLoop _audioSource;

    private void Awake()
    {
      _animator = GetComponent<Animator>();
      _animator.SetBool(IsMoving, false);
      _trailRenderer = GetComponent<TrailRenderer>();
      _spriteRenderer = GetComponent<SpriteRenderer>();
      _material = GetComponent<SpriteRenderer>().material;
      _isFacingRight = GetComponent<SpriteRenderer>().flipX;
      _audioSource = GetComponent<AudioLoop>();
    }

    private void Start()
    {
      var colours = ScriptableObjectsDirector.Instance.colours[0].Colours;
      var randomColour = Random.Range(0, colours.Length);
      _material.SetColor(Colour, colours[randomColour]);
      _trailRenderer.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.CompareTag("Player") == false && other.CompareTag("Bullet") == false) return;
      if (_isMoving)
      {
        DOTween.Kill(parentTransform);
        Move();
        return;
      }

      Move();
    }

    private async void Move()
    {
      _isMoving = true;
      _animator.SetBool(IsMoving, true);
      _trailRenderer.enabled = true;
      _audioSource.SetPlay(true);
      var index = _currentPosition;
      while (index == _currentPosition)
      {
        index = Random.Range(0, waypoints.Length);
      }

      _currentPosition = index;
      _destination = waypoints[index].position;
      var position = parentTransform.position;
      var distance = (_destination - position).magnitude;
      var direction = _destination.x - position.x;
      var duration = distance / mSpeed;
      SetSpriteDirection(direction);
      parentTransform.DOMove(_destination, duration)
        .SetEase(Ease.OutCirc)
        .OnComplete(() =>
        {
          _animator.SetBool(IsMoving, false);
          _isMoving = false;
        });
      var delay = duration / 2;
      _audioSource.FadeVolume(delay, -1, 0, delay);
      await Task.Delay(TimeSpan.FromSeconds(duration));
      if (_isMoving) return;
      _trailRenderer.enabled = false;
    }

    private void SetSpriteDirection(float direction)
    {
      switch (direction)
      {
        case < 0:
          SetIsFacingRight(1);
          break;
        case > 0:
          SetIsFacingRight(0);
          break;
      }
    }

    // Also used by animation event
    private void SetIsFacingRight(int isFacingRight)
    {
      _isFacingRight = isFacingRight == 1;
      _spriteRenderer.flipX = _isFacingRight;
    }

    // Used by animation event only, not used by any script
    private void PlayNextIdleAnimation()
    {
      _animator.SetInteger(Idle, Random.Range(0, 6));
    }
  }
}