using System;
using CaptainHindsight.Core;
using CaptainHindsight.Data.GameStates;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Directors
{
  public class GameStateDirector : MonoBehaviour
  {
    public static GameStateDirector Instance;

    [Title("States")] [ShowInInspector] [ReadOnly]
    private GameState _previousState;

    [SerializeField] private GameState currentState = GameState.Transition;

    [Title("Settings")]
    [AssetList(Path = "/Data/GameStates/", AutoPopulate = true)]
    [SerializeField]
    private GameStateSettings[] stateSettings; // Error state must be 0 in the list

    public static event Action<GameState, GameStateSettings, string> OnGameStateChange;

    private void Awake()
    {
      // Make this class a singleton
      if (Instance == null)
      {
        Instance = this;
        DontDestroyOnLoad(gameObject);
      }
      else
      {
        Destroy(gameObject);
        return;
      }

      // Set previous state to current state to avoid errors
      _previousState = currentState;
    }

    public void SwitchState(GameState state)
    {
      SwitchState(state, string.Empty);
    }

    #region Switching state method which is called by other classes

    public void SwitchState(GameState state, string message)
    {
      if (state == currentState)
      {
        Helper.LogWarning("[GameStateDirector] Game is already in '" + currentState +
                          "' state. No action taken.");
        return;
      }

      // Store previous state before changing so state can be reverted
      _previousState = currentState;

      // Read settings for state
      var stateSettings = ReadStateSettings(state);
      if (stateSettings == null)
      {
        state = GameState.Error;
        stateSettings = ReadStateSettings(state);
        Helper.LogError(
          "[GameStateDirector] State settings do not exist. Error state will be triggered.");
      }

      // For trouble shooting only
      //else DebugStateSettings(stateSettings);

      // Change time scale
      UpdateTimeScale(stateSettings.FreezeTime);

      // Exit current game state...
      switch (currentState)
      {
        case GameState.Tutorial: break;
        case GameState.Play: break;
        case GameState.Pause: break;
        case GameState.GameOver: break;
        case GameState.Win: break;
        case GameState.Transition: break;
        case GameState.Menu: break;
        case GameState.Error: break;
      }

      // Enter the new game state...
      switch (state)
      {
        case GameState.Tutorial: break;
        case GameState.Play: break;
        case GameState.Pause: break;
        case GameState.GameOver: break;
        case GameState.Win: break;
        case GameState.Transition: break;
        case GameState.Menu: break;
        case GameState.Error: break;
        default:
          Helper.LogError("[GameStateDirector] An unknown state was triggered: " + state +
                          ". Switching to Error state.");
          state = GameState.Error;
          break;
      }

      // Set current state to allow next change
      currentState = state;

      // Trigger state change event for anyone who's listening
      OnGameStateChange?.Invoke(state, stateSettings, message);
      Helper.Log("[GameStateDirector] State changed from '" + _previousState + "' to '" +
                 currentState + "' (" +
                 (message == "" ? "no message" : "message='" + message + "'") + ").");
    }

    #endregion

    private GameStateSettings ReadStateSettings(GameState state)
    {
      for (var i = 0; i < stateSettings.Length; i++)
        if (stateSettings[i].Name == state)
          return stateSettings[i];

      return null;
    }

    private void UpdateTimeScale(bool freezeTime)
    {
      Time.timeScale = freezeTime ? 0f : 1f;
    }

    public GameState CurrentState()
    {
      return currentState;
    }

    // Only used for trouble shooting
    private void DebugStateSettings(GameStateSettings currentState)
    {
      for (var i = 0; i < stateSettings.Length; i++)
        if (stateSettings[i].Name == _previousState)
        {
          Helper.Log("[GameStateDirector] Settings for state '" + currentState.Name +
                     "' requested. Details below.");
          Helper.Log(" - Show in-game UI: " + currentState.ShowInGameUI + ".");
          Helper.Log(" - Freeze time: " + currentState.FreezeTime + ".");
          Helper.Log(" - Show in-game UI: " + currentState.ShowInGameUI + ".");
          Helper.Log(" - Player can move: " + currentState.PlayerCanMove + ".");
        }
    }
  }
}