using System.Threading.Tasks;
using CaptainHindsight.Directors;
using CaptainHindsight.Directors.Audio;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.NPCs
{
  public class Bird : MonoBehaviour
  {
    [SerializeField] [ProgressBar(1, 10)] private int timeToFlyOff = 2;

    [SerializeField] [ProgressBar(0, 60, 1, 1, 0)]
    private int midAirDelay = 2;

    [SerializeField] [ProgressBar(1, 20)] private int timeToLandAgain = 5;

    private Animator _animator;
    private Vector3 _currentPosition;
    private bool _isFacingRight;
    private float _previouslyTriggered;
    private float _swarmDifferentiator;
    private Material _material;
    private AudioOneShot _audioOneShot;
    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int Fly = Animator.StringToHash("Fly");
    private static readonly int Colour = Shader.PropertyToID("_Colour");

    private void Awake()
    {
      _animator = GetComponent<Animator>();
      _isFacingRight = GetComponent<SpriteRenderer>().flipX;
      _material = GetComponent<SpriteRenderer>().material;
      _audioOneShot = GetComponent<AudioOneShot>();
      _currentPosition = transform.position;
    }

    private void Start()
    {
      // Set animation to idle
      _animator.SetBool(Fly, false);

      // Set random colour
      var colours = ScriptableObjectsDirector.Instance.colours[0].Colours;
      var randomColour = Random.Range(0, colours.Length);
      _material.SetColor(Colour, colours[randomColour]);

      // Set random differentiator to delay/speed up actions
      _swarmDifferentiator = Random.Range(-0.7f, 0.7f);

      // Helper.Log("[" + transform.parent.parent.name + "] Random colour selected: " + randomColour + "/" + (colours.Length - 1) + " - swarmDif " + swarmDifferentiator + " .");
    }

    // Animation event only, not used by any script
    private void PlayNextIdleAnimation()
    {
      _animator.SetInteger(Idle, Random.Range(0, 5));
    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Bullet"))
      {
        FlyOff();
        LandAgain();
      }
    }

    private async void FlyOff()
    {
      _animator.SetBool(Fly, true);
      var randomX = Random.Range(-15f, 15f);
      var randomZ = Random.Range(-10f, 0f);
      if (_previouslyTriggered > 0f)
        randomX = 0f; // Don't change X, if previously triggered to avoid sprite flipping issues


      var destination = new Vector3(randomX, 15f, randomZ);
      SetSpriteDirection(randomX);
      _previouslyTriggered = Time.fixedTime;

      await Task.Delay(System.TimeSpan.FromSeconds(Mathf.Abs(_swarmDifferentiator / 2)));
      _audioOneShot.Play();
      transform.DOLocalMove(destination, timeToFlyOff + _swarmDifferentiator).SetEase(Ease.InSine);
      // Helper.Log("[Bird] Flying off to " + destination + ".");
    }

    private async void LandAgain()
    {
      try
      {
        // Delay landing
        await Task.Delay(
          System.TimeSpan.FromSeconds(timeToFlyOff + midAirDelay + _swarmDifferentiator * 2));

        // Flip sprite and fly back
        FlipSpriteOnXAxis();
        transform.DOMove(_currentPosition, timeToLandAgain + _swarmDifferentiator)
          .SetEase(Ease.InOutSine);
        await Task.Delay(System.TimeSpan.FromSeconds(timeToLandAgain + _swarmDifferentiator));

        // Switch to idle animation if landing was expected
        var expectedLandingTime = _previouslyTriggered + timeToFlyOff + midAirDelay +
                                  timeToLandAgain + _swarmDifferentiator * 3;
        var actualTime = Time.fixedTime;
        if (actualTime >= expectedLandingTime - 0.5f) _animator.SetBool(Fly, false);
        // Helper.Log("[Bird] Landing again. Expected landing: " + expectedLandingTime + " vs actual: " + actualTime + ".");
        // else Helper.Log("[" + transform.parent.parent.name + "] Landing animation requested but request ignored due. Expected landing: " + expectedLandingTime + " vs actual: " + actualTime + ".");
      }
      catch
      {
        // Helper.Log("[Bird] Object no longer exists. Landing cancelled.");
      }
    }

    private void SetSpriteDirection(float moveX)
    {
      switch (moveX)
      {
        case > 0f when !_isFacingRight:
        case < 0f when _isFacingRight:
          FlipSpriteOnXAxis();
          break;
      }
    }

    private void FlipSpriteOnXAxis()
    {
      var spriteRenderer = GetComponent<SpriteRenderer>();
      var currentSetting = spriteRenderer.flipX;
      spriteRenderer.flipX = !currentSetting;
      _isFacingRight = !_isFacingRight;
    }
  }
}