using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHindsight.Core;
using CaptainHindsight.Directors.Audio;
using CaptainHindsight.Managers;
using CaptainHindsight.Other;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Player
{
  [DisallowMultipleComponent]
  public class EquipmentController : MonoBehaviour
  {
    [Title("General")] [Required] [SerializeField]
    private Transform playerManager;

    private AudioOneShot _equipSound;

    [Required] [SerializeField] private List<EquipmentItem> eDataList;
    [Required] [SerializeField] private List<GameObject> ePrefabList;
    private MouseController _mouseController;
    private ObjectPoolManager _objectPoolManager;
    private MCameraShake _cameraShake;

    [Title("Active equipment")]
    [Required]
    [SerializeField]
    [AssetSelector(Paths = "Assets/Sprites/Weapons/", ExpandAllMenuItems = true)]
    private List<Sprite> sprites;

    [Required] [SerializeField] private SpriteRenderer spriteRenderer;
    [Required] [SerializeField] private Transform firePoint;
    private Equipment _activeEquipmentType = Equipment.None;
    [ShowInInspector] [ReadOnly] private int _activeEquipment;
    [ShowInInspector] [ReadOnly] private int _currentDirection;
    [ShowInInspector] [ReadOnly] private int _damage;
    [ShowInInspector] [ReadOnly] private int _projectileCount;
    [ShowInInspector] [ReadOnly] private AudioOneShotRandom _equipmentAudio;

    [Title("Offsets")] [SerializeField] [Required]
    private List<Transform> leftHandTargetList;

    [SerializeField] [Required] private List<Transform> rightHandTargetList;

    [Title("Cooldown")] [ShowInInspector] [ReadOnly]
    private float _attackCooldown;

    private float _attackCooldownTimer = Mathf.Infinity;

    [Header("Recoil")] private float _recoilIntensity;
    private float _recoilTime;

    #region Start & Update

    private void Start()
    {
      _mouseController = MouseController.Instance;
      _objectPoolManager = ObjectPoolManager.Instance;
      _cameraShake = MCameraShake.Instance;
      _equipSound = GetComponent<AudioOneShot>();

      // Deactivate aimTransform once initialisation is complete
      transform.parent.gameObject.SetActive(false);
    }

    private void Update()
    {
      if (_attackCooldownTimer < _attackCooldown)
        _attackCooldownTimer += Time.deltaTime;
    }

    #endregion

    #region Managing direction change

    public void ChangeDirection(int direction, bool priority)
    {
      if (direction == _currentDirection && priority == false) return;

      //Helper.Log("[EquipmentController] Equipment (" + transform.name + ") direction changed to: " + direction);

      _currentDirection = direction;
      spriteRenderer.sprite = sprites[direction];

      switch (direction)
      {
        case 0:
        {
          ePrefabList[_activeEquipment].transform.localPosition = new Vector3(
            ePrefabList[_activeEquipment].transform.localPosition.x,
            ePrefabList[_activeEquipment].transform.localPosition.y, 0.002f);
          var localPosition = firePoint.localPosition;
          localPosition = new Vector3(localPosition.x, 0f, localPosition.z);
          firePoint.localPosition = localPosition;
          break;
        }
        case 1:
        {
          ePrefabList[_activeEquipment].transform.localPosition = new Vector3(
            ePrefabList[_activeEquipment].transform.localPosition.x,
            ePrefabList[_activeEquipment].transform.localPosition.y, -0.0001f);
          var localPosition = firePoint.localPosition;
          localPosition = new Vector3(localPosition.x, 0.005f, localPosition.z);
          firePoint.localPosition = localPosition;
          break;
        }
        case 2:
        {
          ePrefabList[_activeEquipment].transform.localPosition = new Vector3(
            ePrefabList[_activeEquipment].transform.localPosition.x,
            ePrefabList[_activeEquipment].transform.localPosition.y, -0.002f);
          var localPosition = firePoint.localPosition;
          localPosition = new Vector3(localPosition.x, 0f, localPosition.z);
          firePoint.localPosition = localPosition;
          break;
        }
        case 3:
        {
          ePrefabList[_activeEquipment].transform.localPosition = new Vector3(
            ePrefabList[_activeEquipment].transform.localPosition.x,
            ePrefabList[_activeEquipment].transform.localPosition.y, 0.0001f);
          var localPosition = firePoint.localPosition;
          localPosition = new Vector3(localPosition.x, -0.02f, localPosition.z);
          firePoint.localPosition = localPosition;
          break;
        }
      }
    }

    #endregion

    #region Managing equipment change

    public List<EquipmentItem> GetEquipmentList()
    {
      return eDataList;
    }

    public int GetEquipmentQuantity()
    {
      return eDataList.Count;
    }

    public void ChangeEquipment(int input)
    {
      // Map input to equipmentList and equipmentItemList
      var slot = input - 1;

      // Update active equipment
      _activeEquipmentType = eDataList[slot].Type;
      _activeEquipment = slot;

      // Deactivate all GameObjects except for the selected equipment
      ePrefabList.ForEach(item => item.SetActive(false));
      ePrefabList[slot].SetActive(true);

      // Get references for active equipment
      spriteRenderer = ePrefabList[slot].GetComponent<SpriteRenderer>();
      firePoint = ePrefabList[slot].transform.Find("FirePoint");

      // Update offsets for each hand and direction
      leftHandTargetList[0].localPosition = eDataList[slot].LHandTargetN;
      leftHandTargetList[1].localPosition = eDataList[slot].LHandTargetW;
      leftHandTargetList[2].localPosition = eDataList[slot].LHandTargetS;
      leftHandTargetList[3].localPosition = eDataList[slot].LHandTargetE;
      rightHandTargetList[0].localPosition = eDataList[slot].RHandTargetN;
      rightHandTargetList[1].localPosition = eDataList[slot].RHandTargetW;
      rightHandTargetList[2].localPosition = eDataList[slot].RHandTargetS;
      rightHandTargetList[3].localPosition = eDataList[slot].RHandTargetE;

      // Update sprites to current equipment
      sprites.Clear();
      sprites.Add(eDataList[slot].Sprites.North);
      sprites.Add(eDataList[slot].Sprites.West);
      sprites.Add(eDataList[slot].Sprites.South);
      sprites.Add(eDataList[slot].Sprites.East);

      // Run ActionDirectionChange to update sprite and offset sprite transform
      ChangeDirection(_currentDirection, true);

      // Update recoil
      _recoilIntensity = eDataList[slot].RecoilIntensity;
      _recoilTime = eDataList[slot].RecoilTime;

      // Update other equipment stats
      _damage = eDataList[slot].DamageBase;
      _projectileCount = eDataList[slot].ProjectileCount;
      _attackCooldown = eDataList[slot].AttackCooldown;

      // Update cursor
      EventManager.Instance.ChangeCursor(eDataList[slot].Cursor);

      // Set SFX and play equip sound
      _equipmentAudio = ePrefabList[slot].GetComponent<AudioOneShotRandom>();
      _equipSound.Play();

      // Update stats based on active skills
      UpdateActiveSkills();

      // For trouble-shooting
      //Helper.Log("[EquipmentController] Slot: " + slot + " | " + activeEquipmentType + ".");
    }

    public float GetWalkSpeedModifier(int input)
    {
      return eDataList[input - 1].WalkSpeedModifier;
    }

    private void UpdateActiveSkills()
    {
      // Update stats based on active skills
      PlayerSkillsManager.Instance
        .GetActiveSkills(_activeEquipmentType)
        .ForEach(s =>
        {
          if (s.Type == Skill.SkillType.FireRate)
            _attackCooldown = s.Value;
          if (s.Type == Skill.SkillType.Projectiles)
            _projectileCount = (int)s.Value;
        });
    }

    #endregion

    #region Managing attack

    public void Attack(Vector3 playerPosition)
    {
      if (_attackCooldownTimer < _attackCooldown) return;

      // For troubleshooting
      // Helper.Log("[EquipmentController] Attack received!");
      // Debug.DrawLine(firePoint.position, mouseController.MPRaw, Color.white, .1f);

      // Initiate attack
      _attackCooldownTimer = 0;
      _cameraShake.ShakeCamera(_recoilIntensity, _recoilTime);
      InstantiateMuzzleFlash();
      _equipmentAudio.Play();

      // Instantiate bullet
      var fpp = firePoint.position;
      var modifiedFirePoint = new Vector3(fpp.x,
        Mathf.Clamp(fpp.y, playerPosition.y + 0.4f, playerPosition.y + 1f),
        fpp.z); // Ensures collision with player collider are impossible
      var direction = _mouseController.MpRaw - modifiedFirePoint;
      var normalisedDirection = direction.normalized;
      for (var i = 0; i < _projectileCount; i++)
      {
        InstantiateBullet(modifiedFirePoint, normalisedDirection, direction);
      }
    }

    private async void InstantiateMuzzleFlash()
    {
      var fpt = firePoint.transform;
      var muzzleFlash = _objectPoolManager.SpawnFromPool("muzzleFlash",
        fpt.position, fpt.rotation);
      muzzleFlash.transform.parent = firePoint.transform;
      await Task.Delay(System.TimeSpan.FromSeconds(0.08f));
      muzzleFlash.SetActive(false);
    }

    private void InstantiateBullet(Vector3 position, Vector3 normDirection, Vector3 direction)
    {
      var bulletObject = _objectPoolManager
        .SpawnFromPool("ammo-Bullet", position, firePoint.transform.rotation);
      bulletObject
        .GetComponent<Bullet>()
        .Initialise(playerManager, normDirection, direction.magnitude >= 1.3f, _damage);
    }

    #endregion

    #region Managing events

    private void OnEnable()
    {
      EventManager.Instance.OnActiveSkillsChange += UpdateActiveSkills;
    }

    private void OnDisable()
    {
      EventManager.Instance.OnActiveSkillsChange -= UpdateActiveSkills;
    }

    #endregion
  }
}