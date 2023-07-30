using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHindsight.Core;
using CaptainHindsight.Directors;
using CaptainHindsight.Managers;
using CaptainHindsight.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CaptainHindsight.NPCs
{
  public class EnemyWaveController : MonoBehaviour
  {
    [Title("Wave Event", "General")] [ShowInInspector] [ReadOnly]
    private string _id;

    [SerializeField] [Required] private string eventName;
    [ShowInInspector] [ReadOnly] private bool _eventCompleted;
    [ShowInInspector] [ReadOnly] private bool _eventInProgress;
    [ShowInInspector] [ReadOnly] private bool _eventInterval;
    [ShowInInspector] [ReadOnly] private int _totalWaves;
    [ShowInInspector] [ReadOnly] private int _currentWave;
    [ShowInInspector] [ReadOnly] private bool _bannerTriggerLinked;

    [ShowInInspector] [ReadOnly] [ShowIf("_bannerTriggerLinked")]
    private AreaZoneTrigger _bannerBarTrigger;

    [Title("Wave Event", "Active Wave")] [ShowInInspector] [ReadOnly]
    private int _enemiesSpawned;

    [ShowInInspector] [ReadOnly] private int _enemiesCurrentlyActive;
    [ShowInInspector] [ReadOnly] private int _enemiesKilled;
    [ShowInInspector] [ReadOnly] private int _enemiesToSpawn;
    [ShowInInspector] [ReadOnly] private float _waveLengthTimer;
    [ShowInInspector] [ReadOnly] private float _spawnIntervalTimer;

    [Title("Spawn Points")] [SerializeField] [Required]
    private List<Transform> spawnPoints;

    private Transform _player;

    [Title("Wave Settings")]
    [InfoBox(
      "Priority for each enemy is calculated by dividing the priority by the sum of all probabilities of all enemies in the current wave.")]
    [InfoBox(
      "The 'Additional per Wave' property adds additional enemies + max concurrent enemies per wave for enemy-based waves and additional seconds to survive + max concurrent enemies per wave for time-based waves.")]
    [SerializeField]
    [Required]
    private List<Wave> waves;

    #region Awake() and initialisation

    private void Awake()
    {
      _id = "EVENT-" + GetInstanceID();
      TryGetMBannerBarTriggerComponent();
      _totalWaves = waves.Count - 1;
      SetProbabilityRanges();
      InitialiseCurrentWave();
    }

    private void TryGetMBannerBarTriggerComponent()
    {
      transform.parent.TryGetComponent<AreaZoneTrigger>(out var component);
      if (component == null)
      {
        Helper.LogWarning(
          "[EnemyWaveController] Cannot find AreaZoneTrigger on the same game object. WaveEventController may not behave as expected.");
        return;
      }

      _bannerBarTrigger = component;
      _bannerTriggerLinked = true;
      Helper.Log("[EnemyWaveController] Successfully linked AreaZoneTrigger component.");
    }

    private void SetProbabilityRanges()
    {
      // Sets the weights and therefore probability for each enemy in each wave 
      // to spawn on Awake()
      for (var i = 0; i < waves.Count; i++)
      {
        var minRange = 1;
        var maxRange = waves[i].enemies[0].weight;
        for (var j = 0; j < waves[i].enemies.Count; j++)
        {
          waves[i].enemies[j].SetRange(minRange, maxRange);
          minRange = minRange + maxRange;
          if (j + 1 < waves[i].enemies.Count)
            maxRange = maxRange + waves[i].enemies[j + 1].weight;
        }
      }
    }

    #endregion

    private void OnTriggerEnter(Collider other)
    {
      if (other.CompareTag("Player") == false) return;

      _player = other.transform;

      if (_eventInProgress == false && _eventCompleted == false)
      {
        if (_bannerTriggerLinked) _bannerBarTrigger.Configure(false, true, this);

        StartEvent();
        if (_bannerTriggerLinked)
          _bannerBarTrigger.UpdateMessageText("In progress: Wave " + (_currentWave + 1));
      }
    }

    private void Update()
    {
      // Don't do anything if the event is not in progress or the timer is running
      if (_eventInProgress == false || _eventInterval) return;

      // Action current wave - type: Time-based wave
      if (waves[_currentWave].timeBasedWave)
      {
        _waveLengthTimer -= Time.deltaTime;
        _spawnIntervalTimer -= Time.deltaTime;

        // Trigger next wave or end event when necessary
        if (_waveLengthTimer <= 0)
        {
          if (_enemiesCurrentlyActive > 0) return;
          TriggerNextWave();
          return;
        }

        // Spawn enemies based on interval and probability
        if (_spawnIntervalTimer <= 0 &&
            _enemiesCurrentlyActive < waves[_currentWave].maxConcurrentEnemies)
        {
          SpawnEnemy();
          _spawnIntervalTimer = waves[_currentWave].spawnInterval;
        }
      }
      // Action current wave - type: Kill-based wave
      else
      {
        // Spawn enemies until maxConcurrentEnemies is hit or enemiesToSpawn (in current wave) is 0
        if (_enemiesToSpawn > 0 &&
            _enemiesCurrentlyActive < waves[_currentWave].maxConcurrentEnemies)
          SpawnEnemy();

        // Trigger next wave or end wave when necessary
        if (_enemiesKilled >= waves[_currentWave].enemiesToKill)
          TriggerNextWave();
      }
    }

    private void SpawnEnemy()
    {
      var nextEnemy = UnityEngine.Random.Range(1, waves[_currentWave].SumOfWeights + 1);

      for (var j = 0; j < waves[_currentWave].enemies.Count; j++)
        if (waves[_currentWave].enemies[j].CheckRange(nextEnemy))
        {
          // Select spawnPoint, calculate offset and spawn enemy
          var proposedSpawnPoint = _player.transform.position;
          while ((proposedSpawnPoint - _player.transform.position).magnitude < 12f)
          {
            var nextSpawnPoint = UnityEngine.Random.Range(0, spawnPoints.Count);
            var offset = new Vector3(UnityEngine.Random.Range(0f, 2f), 0,
              UnityEngine.Random.Range(0f, 2f));
            proposedSpawnPoint = spawnPoints[nextSpawnPoint].position + offset;
          }

          // For debugging:
          // Helper.Log("[EnemyWaveController] Spawning enemy (distance to player: " + (proposedSpawnPoint - player.transform.position).magnitude + ").");
          var enemy = Instantiate(waves[_currentWave].enemies[j].prefab, proposedSpawnPoint,
            Quaternion.identity, null);

          // Set enemy's cooperation target to the player
          var mRemoteSetup = enemy.AddComponent<MRemoteSetup>();
          mRemoteSetup.SetCooperationTarget(_player);

          // Attach component that allows receiving wave event data via EventManager
          var mEventDefeatable = enemy.GetComponentInChildren<HealthManager>()
            .gameObject.AddComponent<MEventDefeatable>();
          mEventDefeatable.SetEventIdentifier(_id);
          mEventDefeatable.SetNpcIdentifier(waves[_currentWave].enemies[j].identifier);
        }

      _enemiesCurrentlyActive++;
      _enemiesSpawned++;
      if (_enemiesToSpawn > 0) _enemiesToSpawn--;
    }

    private void StartEvent()
    {
      if (_eventInProgress)
      {
        Helper.LogWarning(
          "[EnemyWaveController] Something triggered an event that is already in progress. Trigger has been ignored.");
        return;
      }

      Helper.Log("[EnemyWaveController] Starting a new enemy wave event...");
      if (_bannerTriggerLinked)
      {
        _bannerBarTrigger.UpdateMessageText(eventName + " starting now");
        _bannerBarTrigger.TriggerBannerBar();
      }

      _eventInProgress = true;
      TriggerCountdown();
    }

    private void InitialiseCurrentWave()
    {
      if (_eventCompleted) return;
      Helper.Log("[EnemyWaveController] Initialising wave.");

      _waveLengthTimer = waves[_currentWave].secondsToSurvive + 3;
      _spawnIntervalTimer = waves[_currentWave].spawnInterval;
      _enemiesSpawned = 0;
      _enemiesCurrentlyActive = 0;
      _enemiesKilled = 0;
      _enemiesToSpawn = waves[_currentWave].enemiesToKill;
    }


    private async void TriggerCountdown()
    {
      _eventInterval = true;
      await Task.Delay(TimeSpan.FromSeconds(3.5f));
      if (_bannerTriggerLinked) _bannerBarTrigger.TriggerCountdown(3);
      var elapsedTime = 0f;
      while (elapsedTime < 3f)
      {
        elapsedTime += Time.deltaTime;
        await Task.Yield();
      }
      _eventInterval = false;
    }

    private void TriggerNextWave()
    {
      // If all waves are completed, end the event...
      if (waves[_currentWave].infiniteLoop == false && _currentWave == _totalWaves)
      {
        Helper.Log("[EnemyWaveController] All waves completed.");
        _eventCompleted = true;
        _eventInProgress = false;
        if (_bannerTriggerLinked)
        {
          _bannerBarTrigger.Configure(true);
          _bannerBarTrigger.UpdateMessageText(eventName + " completed");
          _bannerBarTrigger.TriggerBannerBar("Completion");
          _bannerBarTrigger.UpdateMessageText("");
        }

        Destroy(gameObject);
        return;
      }

      // ...otherwise initialise next wave and display banner message
      AddWaveIfCurrentWaveIsInfiniteLoop();
      _currentWave++;
      if (_bannerTriggerLinked)
      {
        _bannerBarTrigger.UpdateMessageText("Wave " + (_currentWave + 1));
        _bannerBarTrigger.TriggerBannerBar();
        _bannerBarTrigger.PrependMessageText("In progress: ");
      }

      InitialiseCurrentWave();
      TriggerCountdown();
    }

    private void AddWaveIfCurrentWaveIsInfiniteLoop()
    {
      if (!waves[_currentWave].infiniteLoop) return;
      var wave = new Wave
      {
        timeBasedWave = waves[_currentWave].timeBasedWave,
        infiniteLoop = true,
        enemiesToKill = waves[_currentWave].enemiesToKill + waves[_currentWave].additionalPerWave,
        additionalPerWave = waves[_currentWave].additionalPerWave,
        maxConcurrentEnemies = waves[_currentWave].maxConcurrentEnemies +
                               waves[_currentWave].additionalPerWave,
        secondsToSurvive =
          waves[_currentWave].secondsToSurvive + waves[_currentWave].additionalPerWave,
        spawnInterval = waves[_currentWave].spawnInterval,
        enemies = waves[_currentWave].enemies
      };
      waves.Add(wave);
    }

    #region Managing events

    // Triggered by MEventDefeatable on death of an enemy
    private void ActionWaveEvent(string eventId, NpcIdentifier npcIdentifier)
    {
      if (_id != eventId) return;
      _enemiesCurrentlyActive--;
      _enemiesKilled++;
    }

    private void OnEnable()
    {
      EventManager.Instance.OnWaveEvent += ActionWaveEvent;
    }

    private void OnDestroy()
    {
      EventManager.Instance.OnWaveEvent -= ActionWaveEvent;
    }

    public void PauseEvent(bool status = true)
    {
      _eventInterval = status;
    }

    #endregion

    #region Managing classes for wave event

    [Serializable]
    public class Wave
    {
      [Title("Base settings")] [LabelWidth(160)]
      public bool timeBasedWave;

      [LabelWidth(160)] public bool infiniteLoop;

      [LabelWidth(160)] [HideIf("timeBasedWave")]
      public int enemiesToKill;

      [LabelWidth(160)] [ShowIf("timeBasedWave")] [MinValue(5)]
      public int secondsToSurvive;

      [LabelWidth(160)] [ShowIf("infiniteLoop")] [MinValue(1)]
      public int additionalPerWave = 1;

      [LabelWidth(160)] [ShowIf("timeBasedWave")] [MinValue(1)]
      public int spawnInterval = 1;

      [LabelWidth(160)] public int maxConcurrentEnemies = 5;

      [TitleGroup("Enemy settings")]
      [LabelWidth(160)]
      [ShowInInspector]
      [ReadOnly]
      public int SumOfWeights
      {
        get
        {
          var value = 0;
          foreach (var enemy in enemies) value += enemy.weight;

          return value;
        }
      }

      [TitleGroup("Enemy settings")] [LabelWidth(160)]
      public List<WaveEnemy> enemies;
    }

    [Serializable]
    public class WaveEnemy
    {
      [FormerlySerializedAs("Prefab")]
      [LabelWidth(80)]
      [AssetSelector(Paths = "Assets/Prefabs/NPCs/")]
      public GameObject prefab;

      public NpcIdentifier identifier;

      [Range(1, 100)] public int weight = 1;

      [ShowInInspector] [ReadOnly] private int _x;

      [ShowInInspector] [ReadOnly] private int _y;

      public void SetRange(int x, int y)
      {
        _x = x;
        _y = y;
      }

      public bool CheckRange(int number)
      {
        if (number >= _x && number <= _y) return true;
        return false;
      }
    }

    #endregion
  }
}