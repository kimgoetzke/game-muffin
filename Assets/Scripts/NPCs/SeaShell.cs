using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CaptainHindsight.Directors;
using CaptainHindsight.Directors.Audio;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CaptainHindsight.NPCs
{
  public class SeaShell : MonoBehaviour
  {
    [TitleGroup("Base settings")] [SerializeField]
    private bool underWater;

    [TitleGroup("References")] [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite shellOpen;
    [SerializeField] private Sprite shellClosed;
    [SerializeField] private AudioOneShot atShoreOpen;
    [SerializeField] private AudioOneShot underWaterOpen;
    [SerializeField] private AudioOneShot atShoreClose;
    [SerializeField] private AudioOneShot underWaterClose;
    private CancellationTokenSource _cancellationTokenSource;
    [ShowInInspector, ReadOnly] private List<Transform> _enemies = new();
    [ShowInInspector, ReadOnly] private bool _isOpen = true;
    private Material _material;
    private static readonly int Colour = Shader.PropertyToID("_Colour");

    private void Start()
    {
      spriteRenderer = GetComponent<SpriteRenderer>();
      spriteRenderer.flipX = Random.Range(0, 2) == 0;
      _material = GetComponent<SpriteRenderer>().material;
      var colours = ScriptableObjectsDirector.Instance.colours[2].Colours;
      var randomColour = Random.Range(0, colours.Length);
      _material.SetColor(Colour, colours[randomColour]);
    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.CompareTag("Bullet"))
      {
        UpdateCancellationTokenSource();
        CloseShell();
        OpenShell(Random.Range(1f, 2f));
      }
      else if (other.CompareTag("Player") || 
               other.CompareTag("NPC") ||
               other.CompareTag("Creature"))
      {
        _enemies.Add(other.transform);
        if (_isOpen == false) return;
        UpdateCancellationTokenSource();
        CloseShell();
      }
    }

    private void OnTriggerExit(Collider other)
    {
      if (other.CompareTag("Player") == false && 
          other.CompareTag("NPC") == false &&
          other.CompareTag("Creature") == false) return;
      _enemies.Remove(other.transform);
      UpdateCancellationTokenSource();
      OpenShell();
    }

    private async void OpenShell(float delay = 1f)
    {
      try
      {
        await Task.Delay(System.TimeSpan.FromSeconds(delay), _cancellationTokenSource.Token);
        if (_isOpen || IsEnemyInRange()) return;
        spriteRenderer.sprite = shellOpen;
        if (underWater)
        {
          underWaterOpen.Play();
        }
        else
        {
          atShoreOpen.Play();
        }

        _isOpen = true;
      }
      catch (Exception)
      {
        // Deliberately ignored
      }
    }

    private void CloseShell()
    {
      spriteRenderer.sprite = shellClosed;
      if (underWater)
      {
        underWaterClose.Play();
      }
      else
      {
        atShoreClose.Play();
      }

      _isOpen = false;
    }

    private bool IsEnemyInRange()
    {
      return _enemies.Count > 0;
    }

    private void UpdateCancellationTokenSource()
    {
      _cancellationTokenSource?.Cancel();
      _cancellationTokenSource = new CancellationTokenSource();
    }
  }
}