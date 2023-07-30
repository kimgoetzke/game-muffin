using CaptainHindsight.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace CaptainHindsight.Other
{
  public class MSpriteZSort : MonoBehaviour
  {
    [SerializeField] private SortSettings settings = SortSettings.StaticSprite;

    [SerializeField]
    [Tooltip(
      "Specify where th Sprite Renderer or Sorting Group component for the calculation is located")]
    private bool findComponentInChild;

    [SerializeField]
    [Tooltip("Use a transform that is not the pivot for the basis of the calculation")]
    private Transform sortOffsetMarker;

    [SerializeField]
    [Tooltip(
      "Add an order layer after the calculation i.e. in addition to and after using any sortOffsetMarker")]
    private int additionalLayerOffset;

    private SortingGroup _sortingGroup;
    private SpriteRenderer _spriteRenderer;
    private ParticleSystemRenderer _particleSystemRenderer;
    private float _sortingOffset;

    private void Awake()
    {
      switch (settings)
      {
        case SortSettings.StaticSprite:
          if (findComponentInChild) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
          else _spriteRenderer = GetComponent<SpriteRenderer>();
          _spriteRenderer.sortingOrder = transform.GetSortingOrder() + additionalLayerOffset;
          enabled = false;
          break;
        case SortSettings.StaticSortingGroup:
          if (findComponentInChild) _sortingGroup = GetComponentInChildren<SortingGroup>();
          else _sortingGroup = GetComponent<SortingGroup>();
          _sortingGroup.sortingOrder = transform.GetSortingOrder() + additionalLayerOffset;
          enabled = false;
          break;
        case SortSettings.StaticParticleSystem:
          if (findComponentInChild)
            _particleSystemRenderer = GetComponentInChildren<ParticleSystemRenderer>();
          else _particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
          _particleSystemRenderer.sortingOrder =
            transform.GetSortingOrder() + additionalLayerOffset;
          enabled = false;
          break;
        case SortSettings.DynamicSpriteWithNoOffset:
          if (findComponentInChild) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
          else _spriteRenderer = GetComponent<SpriteRenderer>();
          break;
        case SortSettings.DynamicSpriteUsingMarker:
          if (findComponentInChild) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
          else _spriteRenderer = GetComponent<SpriteRenderer>();
          _sortingOffset = -sortOffsetMarker.position.y;
          break;
        case SortSettings.DynamicSortGroupWithNoOffset:
          if (findComponentInChild) _sortingGroup = GetComponentInChildren<SortingGroup>();
          else _sortingGroup = GetComponent<SortingGroup>();
          break;
        case SortSettings.DynamicSortGroupUsingMarker:
          if (findComponentInChild) _sortingGroup = GetComponentInChildren<SortingGroup>();
          else _sortingGroup = GetComponent<SortingGroup>();
          _sortingOffset = -sortOffsetMarker.position.y;
          break;
        default:
          Helper.LogError("[MSpriteZSort] " + transform.name + " (child of " +
                          transform.parent.name +
                          ") uses MSpriteZSort but its settings are invalid. Find the component and update its settings.");
          enabled = false;
          break;
      }
    }

    private void Update()
    {
      switch (settings)
      {
        case SortSettings.DynamicSpriteWithNoOffset:
          _spriteRenderer.sortingOrder = transform.GetSortingOrder() + additionalLayerOffset;
          break;
        case SortSettings.DynamicSpriteUsingMarker:
          _spriteRenderer.sortingOrder =
            transform.GetSortingOrder(_sortingOffset) + additionalLayerOffset;
          break;
        case SortSettings.DynamicSortGroupWithNoOffset:
          _sortingGroup.sortingOrder = transform.GetSortingOrder() + additionalLayerOffset;
          break;
        case SortSettings.DynamicSortGroupUsingMarker:
          _sortingGroup.sortingOrder =
            transform.GetSortingOrder(_sortingOffset) + additionalLayerOffset;
          break;
        default:
          Helper.LogError("[MSpriteZSort] " + transform.name + " (child of " +
                          transform.parent.name +
                          ") uses MSpriteZSort but its settings are invalid. The component was disabled.");
          enabled = false;
          break;
      }
    }
  }

  public enum SortSettings
  {
    StaticSprite,
    StaticSortingGroup,
    StaticParticleSystem,
    DynamicSpriteWithNoOffset,
    DynamicSpriteUsingMarker,
    DynamicSortGroupWithNoOffset,
    DynamicSortGroupUsingMarker
  }
}