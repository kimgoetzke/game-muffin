using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CaptainHindsight.Core;
using CaptainHindsight.Data.GameStates;
using CaptainHindsight.Data.Skills;
using CaptainHindsight.Directors;
using CaptainHindsight.Managers;
using CaptainHindsight.Other;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D.IK;

namespace CaptainHindsight.Player
{
  [DisallowMultipleComponent]
  public class PlayerController : GameStateBehaviour
  {
    [Header("General")] private Rigidbody _rb;
    private Animator _playerAnimator;
    private AnimationClip _currentAnimation;
    private readonly Dictionary<int, string> _animationDirectionsList = new();
    private bool _isDead;
    private static readonly int MoveSpeed = Animator.StringToHash("moveSpeed");
    private static readonly int DieAnimation = Animator.StringToHash("die");

    [Title("Skeletons")] [SerializeField] private Transform[] playerSkeletons;
    private Transform _currentSkeleton;

    [Title("Solvers")] [Required] [SerializeField]
    private Solver2D[] rightHandSolvers;

    [Required] [SerializeField] private Solver2D[] leftHandSolvers;
    [Required] [SerializeField] private Solver2D[] rightArmSolvers;
    [Required] [SerializeField] private Solver2D[] leftArmSolvers;

    [Title("Movement")] 
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float mSpeed;
    [SerializeField] private float mMaxSpeed;
    [ShowInInspector, ReadOnly] private float _mEquipmentModifier = 1f;
    [SerializeField] private float mSlopeAngle = 45f;
    private PlayerInputActions _playerInputActions;
    private InputAction _moveInput;
    private int _currentDirection;
    private bool _canMove;
    private bool _isMoving;
    private bool _isMovingBackwards;
    private MouseController _mouseController;
    private bool _isMouseControlled;

    [Title("Equipment")] private int _currentEquipmentSlot = 1;
    private int _availableEquipmentSlots;

    [Required] [SerializeField] [ReadOnly]
    private Transform aimTransform; // Only used for parenting and to switch on/off

    [Required] [SerializeField] [ReadOnly] private Transform
      equipmentTransform; // Used for aiming offsets to produce more realistic aim circle

    private EquipmentController _equipmentController;
    private bool _isEquipped;
    private bool _isAttacking;
    private bool _canShoot = true;

    [Header("Equipment transform")] private float _lastFrameXOffset;
    private float _offsetX;
    private float _offsetY;

    [Title("Interaction")] [SerializeField]
    private LayerMask interactionLayer;

    [Title("Skills")] [SerializeField] private PlayerManager playerManager;
    private PlayerSkillsManager _playerSkills;
    private float _healthRegenTimer;
    private readonly float _healthRegenCooldown = 2f;

    #region Awake & Start

    protected override void Awake()
    {
      base.Awake();
      SubscribeToEvents();
      
      // Deactivate all skeletons
      foreach (var skeleton in playerSkeletons) skeleton.gameObject.SetActive(false);

      // Get references
      _rb = GetComponent<Rigidbody>();
      _equipmentController = equipmentTransform.GetComponent<EquipmentController>();
      _mEquipmentModifier = _equipmentController.GetWalkSpeedModifier(1);
      _availableEquipmentSlots = _equipmentController.GetEquipmentQuantity();

      // Create list of directions and activate default skeleton
      CreateListOfDirectionalAnimations();
      _currentSkeleton = playerSkeletons[_currentDirection];
      UpdateSkeleton(2); // Player faces south

      // No equipment active but aimTransform not deactivated
      UpdateSolverWeights(false);
    }

    private void Start()
    {
      _mouseController = MouseController.Instance;
      _playerSkills = PlayerSkillsManager.Instance;
    }

    private void CreateListOfDirectionalAnimations()
    {
      _animationDirectionsList.Add(0, "moveN");
      _animationDirectionsList.Add(1, "moveW");
      _animationDirectionsList.Add(2, "moveS");
      _animationDirectionsList.Add(3, "moveE");
    }

    #endregion

    private void FixedUpdate()
    {
      if (_canMove)
      {
        var lookDirection = _mouseController.mpGroundLevel - _rb.position;
        var skeletalDirection = _currentDirection;
        if (_isEquipped)
        {
          skeletalDirection = Helper.ConvertDirectionToIndex(lookDirection);
        }
        else if (_rb.velocity.sqrMagnitude > 0.01f)
        {
          skeletalDirection = Helper.ConvertDirectionToIndex(_rb.velocity);
        }

        // Use correct skeletal rig
        UpdateSkeleton(skeletalDirection);

        if (_isEquipped)
        {
          // Adjust equipment sprite based on look direction
          UpdateEquipment(skeletalDirection);

          // Aim weapon at mouse
          AimAtMouse(skeletalDirection, lookDirection, out var angle);

          // Adjust position of aim transform depending on direction, speed and status
          OffsetPositionOfEquipmentTransform(lookDirection, angle, skeletalDirection);
        }

        // Make sure player cannot walk step slopes and slides down, if appropriate
        // SlideSlopes();
        
        if (_isMoving)
        {
          // Move player using any input method
          Move(lookDirection);
        }
      }

      // Keep attacking while attack button is pressed
      Attack();

      // Add health if player has health regen skills
      RegenerateHealth();

      // Update player animator (regardless of movement, so that idle animations are played too)
      _playerAnimator.SetFloat(MoveSpeed,
        _isMovingBackwards ? _rb.velocity.magnitude * -1 : _rb.velocity.magnitude);
    }

    #region Managing skeleton and player animator

    private void UpdateSkeleton(int direction)
    {
      if (direction == _currentDirection) return;

      // Update current direction
      _currentDirection = direction;

      // Update solver weights
      UpdateSolverWeights(aimTransform.gameObject.activeInHierarchy);

      // Get normalised time for current animation clip to be able to resume
      float time = 0;
      if (_playerAnimator != null)
      {
        time = _playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
      }

      // Deactivate current animator and skeleton
      _currentSkeleton.gameObject.SetActive(false);
      playerSkeletons[_currentDirection].gameObject.SetActive(true);
      _currentSkeleton = playerSkeletons[_currentDirection];

      // Update animator and animation
      _playerAnimator = playerSkeletons[_currentDirection].GetComponent<Animator>();
      _playerAnimator.enabled = true;
      _playerAnimator.Play(_animationDirectionsList[_currentDirection], 0, time);
      // Helper.Log("[PlayerController] Changing animation to " + directionsList[currentDirection] + ".");

      // Update aim transform
      aimTransform.parent = _currentSkeleton;

      // Reset equipment transform which is adjusted when looking NW, N or NE
      equipmentTransform.localPosition = new Vector3(equipmentTransform.localPosition.x, 0.75f, 0);
    }

    private void UpdateSolverWeights(bool equipmentUsed)
    {
      // Set the weight (only use arm solvers when no equipment is used)
      var weight = equipmentUsed ? 0.01f : 1f;

      // Update arms/hands
      rightArmSolvers[_currentDirection].weight = weight;
      rightHandSolvers[_currentDirection].weight = 1f - weight;
      leftArmSolvers[_currentDirection].weight = weight;
      leftHandSolvers[_currentDirection].weight = 1f - weight;
    }

    #endregion
    
    #region Managing equipment

    private void ChangeEquipment(InputAction.CallbackContext context)
    {
      if (context.phase != InputActionPhase.Performed) return;

      var slot = _currentEquipmentSlot;
      if (context.ReadValue<float>() < 0)
      {
        Equip(Mathf.Clamp(slot - 1, 1, _availableEquipmentSlots));
      }
      else if (context.ReadValue<float>() > 0)
      {
        Equip(Mathf.Clamp(slot + 1, 1, _availableEquipmentSlots));
      }
    }

    private void Equip(int equipmentSlot)
    {
      if (_canShoot == false || _canMove == false) return;
      if (equipmentSlot == _currentEquipmentSlot) return;

      var hasChanged = true;
      if (equipmentSlot == 1)
      {
        _currentEquipmentSlot = equipmentSlot;
        aimTransform.gameObject.SetActive(false);
        _isEquipped = false;
        _isMovingBackwards = false;
        _mEquipmentModifier = _equipmentController.GetWalkSpeedModifier(equipmentSlot);
        UpdateSolverWeights(false);
        EventManager.Instance.ChangeCursor(0);
      }
      else
      {
        if (_playerSkills
            .GetActiveSkills(Skill.SkillType.Handling)
            .Find(s => s.EquipmentSlot == equipmentSlot))
        {
          _currentEquipmentSlot = equipmentSlot;
          aimTransform.gameObject.SetActive(true);
          _isEquipped = true;
          _equipmentController.ChangeEquipment(equipmentSlot);
          _mEquipmentModifier = _equipmentController.GetWalkSpeedModifier(equipmentSlot);
          UpdateSolverWeights(true);
        }
        else
        {
          hasChanged = false;
        }
      }

      if (hasChanged) EventManager.Instance.ChangeEquipment(_currentEquipmentSlot);
    }

    private void UpdateEquipment(int direction)
    {
      _equipmentController.ChangeDirection(direction, false);
    }

    #endregion
    
    #region Managing aiming

    private void AimAtMouse(int skeletalDirection, Vector3 lookDirection, out float angle)
    {
      angle = 0;
      if (skeletalDirection == 0 && lookDirection.magnitude < 1.3f)
      {
        equipmentTransform.eulerAngles = new Vector3(-20f, 0f, 90f);
      }
      else
      {
        var position = transform.position;
        var modifiedPosition = new Vector3(position.x, position.y + 0.75f,
          position.z + 0.5f);
        var aimDirection = (_mouseController.mpGroundLevel - modifiedPosition).normalized;
        angle = Mathf.Atan2(aimDirection.z, aimDirection.x) * Mathf.Rad2Deg;
        equipmentTransform.eulerAngles = new Vector3(45f, 0f, angle + 180f);
      }
    }

    private void OffsetPositionOfEquipmentTransform(Vector3 lookDirection, float angle,
      int skeletalDirection)
    {
      // Set base variables
      var unadjustedXPosition = equipmentTransform.localPosition.x - _lastFrameXOffset;
      var adjustedAngle = Mathf.Abs(angle - 90f) / 90f;
      _offsetX = 0;

      // Move the transform further towards the player when aiming above the player
      if (skeletalDirection == 0) _offsetY = 0.15f * (1 - adjustedAngle);
      else if (adjustedAngle < 1f) _offsetY = 0.075f * (1 - adjustedAngle);
      else _offsetY = 0;

      // Move the transform further away from the player when moving West/East
      if (_isMoving && _isMovingBackwards == false)
      {
        if (_currentDirection == 1)
          _offsetX = Mathf.Clamp01(Mathf.Abs(lookDirection.magnitude) / 2) * -0.1f;
        else if (_currentDirection == 3)
          _offsetX = Mathf.Clamp01(Mathf.Abs(lookDirection.magnitude) / 2) * 0.1f;
      }

      equipmentTransform.localPosition =
        new Vector3(unadjustedXPosition + _offsetX, 0.75f - _offsetY, 0);
      _lastFrameXOffset = _offsetX;
      // Helper.Log("Angle: " + angle + ", offsetX: " + offsetX + ", offsetY: " + offsetY + ".");
    }

    #endregion
    
    #region Managing slopes
    
    private void SlideSlopes()
    {
      RaycastHit[] hitsResults = new RaycastHit[5];
      Physics.RaycastNonAlloc(transform.position, transform.TransformDirection(Vector3.down),
        hitsResults, 2f, groundLayer);

      foreach (var hit in hitsResults)
      {
        // var collider = hit.collider;
        var angle = Vector3.Angle(Vector3.up, hit.normal);
        Debug.DrawLine(hit.point, hit.point + hit.normal, Color.red, 1f);
        Helper.Log(angle.ToString(CultureInfo.InvariantCulture));

        if (angle > mSlopeAngle)
        {
          var normal = hit.normal;
          var yInverse = 1f - normal.y;
          var velocity = _rb.velocity;
          // velocity.x += yInverse * normal.x;
          // velocity.z += yInverse * normal.z;
          velocity = new Vector3(velocity.x + yInverse * normal.x, velocity.y,
            velocity.z + yInverse * normal.z);
          _rb.velocity = velocity;
        }
      }
    }

    #endregion

    #region InputAction: Move

    private void Move(Vector3 lookDirection)
    {
      var minClamp = _isMovingBackwards
        ? -1 * _mEquipmentModifier
        : -mMaxSpeed * _mEquipmentModifier;
      var maxClamp = _isMovingBackwards
        ? 1 * _mEquipmentModifier
        : mMaxSpeed * _mEquipmentModifier;

      if (_isMouseControlled)
      {
        _rb.velocity = new Vector3(
          Mathf.Clamp(lookDirection.x * mSpeed, minClamp, maxClamp),
          _rb.velocity.y,
          Mathf.Clamp(lookDirection.z * mSpeed, minClamp, maxClamp)
        );
      }
      else
      {
        var input = _moveInput.ReadValue<Vector2>();
        var moveDirection = new Vector3(
          Mathf.Clamp(mSpeed * input.x * mMaxSpeed, minClamp, maxClamp),
          _rb.velocity.y,
          Mathf.Clamp(mSpeed * input.y * mMaxSpeed, minClamp, maxClamp)
        );

        if (_isEquipped)
        {
          var dotProduct = Vector3.Dot(lookDirection, moveDirection);
          _isMovingBackwards = dotProduct < 0;
        }

        _rb.velocity = moveDirection;
      }
    }
    #endregion
    
    #region InputAction: Attack

    private void Attack()
    {
      if (_isAttacking == false) return;
      if (_isEquipped == false || _canShoot == false) return;
      _equipmentController.Attack(transform.position);
    }

    #endregion

    #region InputAction: Interact

    private void Interact(InputAction.CallbackContext context)
    {
      var hitColliders = new Collider[5];
      var numColliders =
        Physics.OverlapSphereNonAlloc(transform.position, 4f, hitColliders, interactionLayer);

      for (var i = 0; i < numColliders; i++)
      {
        hitColliders[i].GetComponent<IInteractable>().Interact(transform.position);
      }
    }

    #endregion

    #region InputAction: Pause

    private async void Pause(InputAction.CallbackContext context)
    {
      if (_isDead) return;

      if (context.phase != InputActionPhase.Performed) return;
      await Task.Yield();
      EventManager.Instance.RequestPauseMenu();
    }

    #endregion

    #region Managing skills

    private void RegenerateHealth()
    {
      _healthRegenTimer += Time.deltaTime;

      if (_healthRegenTimer >= _healthRegenCooldown)
      {
        _healthRegenTimer = 0f;
        var activeSkill = _playerSkills.GetActiveSkills(Skill.SkillType.HealthRegen);
        if (activeSkill.Count > 0)
          playerManager.AddHealth((int)activeSkill[0].Value);
      }
    }

    #endregion

    #region Managing death

    private void Die()
    {
      Helper.Log("[PlayerController] Player dies.");
      Equip(1);
      GetComponent<CapsuleCollider>().enabled = false;
      _rb.isKinematic = true;
      _isDead = true;
      _canShoot = false;
      _playerAnimator.SetTrigger(DieAnimation);
    }

    #endregion
    
    #region Managing timeline events
    public void ForceSetSkeletonW()
    {
      UpdateSkeleton(1);
    }

    public void ForceSetSkeletonE()
    {
      UpdateSkeleton(3);
    }
    #endregion

    #region Managing events

    protected override void ActionGameStateChange(GameState state, GameStateSettings settings,
      string message)
    {
      if (state == GameState.GameOver) Die();
      _canMove = settings.PlayerCanMove;
    }

    private void ActionDialogueStateChange(bool state)
    {
      _canShoot = !state;
      _canMove = !state;
      _mouseController.SetCameraTarget(!state);
    }

    private void ActionActiveSkillsChange()
    {
      _availableEquipmentSlots = _equipmentController.GetEquipmentQuantity();
    }

    private void SubscribeToEvents()
    {
      _playerInputActions = new PlayerInputActions();
      _playerInputActions.Player.Enable();
      _playerInputActions.Player.Attack.started += _ => _isAttacking = true;
      _playerInputActions.Player.Attack.canceled += _ => _isAttacking = false;
      _playerInputActions.Player.Interact.performed += Interact;
      _playerInputActions.Player.MoveMouse.started += _ => _isMoving = true;
      _playerInputActions.Player.MoveMouse.started += _ => _isMouseControlled = true;
      _playerInputActions.Player.MoveMouse.canceled += _ => _isMoving = false;
      _playerInputActions.Player.MoveMouse.canceled += _ => _isMouseControlled = false;
      _playerInputActions.Player.Move.started += _ => _isMoving = true;
      _playerInputActions.Player.Move.canceled += _ => _isMoving = false;
      _playerInputActions.Player.Move.canceled += _ => _isMovingBackwards = false;
      _playerInputActions.Player.EquipmentSlot1.performed += _ => Equip(1);
      _playerInputActions.Player.EquipmentSlot2.performed += _ => Equip(2);
      _playerInputActions.Player.EquipmentSlot3.performed += _ => Equip(3);
      _playerInputActions.Player.ChangeEquipment.performed += ChangeEquipment;
      _playerInputActions.Player.Pause.performed += Pause;
      _moveInput = _playerInputActions.Player.Move;
      EventManager.Instance.OnDialogueStateChange += ActionDialogueStateChange;
      EventManager.Instance.OnActiveSkillsChange += ActionActiveSkillsChange;
    }

    protected override void OnDestroy()
    {
      base.OnDestroy();
      _moveInput.Disable();
      _playerInputActions.Player.Interact.performed -= Interact;
      _playerInputActions.Player.Pause.performed -= Pause;
      _playerInputActions.Player.ChangeEquipment.performed -= ChangeEquipment;
      _playerInputActions.Player.Disable();
      EventManager.Instance.OnDialogueStateChange -= ActionDialogueStateChange;
      EventManager.Instance.OnActiveSkillsChange -= ActionActiveSkillsChange;
    }

    #endregion
  }
}