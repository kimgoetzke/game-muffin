using CaptainHindsight.Other;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CaptainHindsight.Managers
{
  public class MouseController : MonoBehaviour
  {
    public static MouseController Instance;
    private Transform _player;
    private Camera _mainCamera;
    private MCameraTarget _cameraTarget;
    private Ray _mouseRay;

    [Title("General settings")] [SerializeField]
    private LayerMask groundLayer;

    [ShowInInspector, ReadOnly] private bool _doesHitGround;
    [ShowInInspector, ReadOnly] public Vector3 MpRaw { get; private set; }

    [ShowInInspector, ReadOnly] public Vector3 mpGroundLevel;

    private void Awake()
    {
      if (Instance == null)
      {
        Instance = this;
      }
      else
      {
        Destroy(gameObject);
      }
    }

    // This method only sets public variables related to the current mouse position -
    // it does not action anything in relation to the mouse position but is used by other methods
    private void FixedUpdate()
    {
      _mouseRay = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
      _doesHitGround = Physics.Raycast(_mouseRay, out var mouseRayHit, 50f,groundLayer);

      if (!_doesHitGround) return;

      // Raw mouse position
      MpRaw = mouseRayHit.point;

      // Modified, setting Y to ground level
      var ptp = _player.transform.position;
      mpGroundLevel = MpRaw;
      mpGroundLevel.y = ptp.y;
    }

    public void SetCameraTarget(bool isEnabled)
    {
      _cameraTarget.enabled = isEnabled;
    }

    private void OnEnable()
    {
      // Find main camera
      if (GameObject.Find("MainCam") == null)
        Debug.LogError("[MouseController] Main camera not found.");
      else _mainCamera = GameObject.Find("MainCam").GetComponent<Camera>();

      // Find player
      if (GameObject.Find("Player") == null)
        Debug.LogError("[MouseController] Player transform not found.");
      else _player = GameObject.Find("Player").transform;

      // Find CmCameraTarget object and look for MCameraTarget component
      if (GameObject.Find("CmCameraTarget") == null)
        Debug.LogError("[MouseController] Camera target not found.");
      else _cameraTarget = GameObject.Find("CmCameraTarget").GetComponent<MCameraTarget>();
    }
  }
}