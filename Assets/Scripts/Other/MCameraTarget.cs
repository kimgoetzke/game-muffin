using CaptainHindsight.Core;
using CaptainHindsight.Managers;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Other
{
  public class MCameraTarget : MonoBehaviour
  {
    [SerializeField] private Transform cmTargetGroup;

    [InfoBox(
      "Note that maxDistanceFromPlayer Y axis is being ignored as the camera is locked to the ground.")]
    [SerializeField]
    private Vector3 maxDistanceFromPlayer = new(8f, 0f, 8f);

    [SerializeField] [Range(1f, 4f)] private float zMultiplier = 3f;
    private MouseController _mouseController;
    private Transform _player;
    private CinemachineVirtualCamera _cinemachineCamera;

    private void Start()
    {
      _mouseController = MouseController.Instance;
    }

    private void Update()
    {
      var targetPosition = _mouseController.mpGroundLevel;
      var playerToMouse = targetPosition - _player.position;
      playerToMouse.x =
        Mathf.Clamp(playerToMouse.x, -maxDistanceFromPlayer.x, maxDistanceFromPlayer.x);
      playerToMouse.z = playerToMouse.z > 0
        ? Mathf.Clamp(playerToMouse.z, 0, maxDistanceFromPlayer.z)
        : Mathf.Clamp(playerToMouse.z * zMultiplier, -maxDistanceFromPlayer.z, 0f);
      targetPosition = _player.position + playerToMouse;
      transform.position = targetPosition;
    }

    private void OnDisable()
    {
      try
      {
        _cinemachineCamera.Follow = _player.transform;
      }
      catch
      {
        Helper.Log(
          "[MCameraTarget] Cinemachine or player no longer exist. Cinemachine target not updated.");
      }
    }

    private void OnEnable()
    {
      // Find player
      if (GameObject.Find("Player") == null)
        Debug.LogError("[MCameraTarget] Player transform not found.");
      else _player = GameObject.Find("Player").transform;

      // Find main Cinemachine camera
      if (GameObject.Find("Cinemachine") == null)
        Debug.LogError("[MCameraTarget] Cinemachine not found.");
      else
        _cinemachineCamera =
          GameObject.Find("Cinemachine").GetComponent<CinemachineVirtualCamera>();

      // Update main Cinemachine camera to use target group
      _cinemachineCamera.Follow = cmTargetGroup;
    }
  }
}