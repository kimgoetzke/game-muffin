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
    private Core.GameState _previousState;

    [SerializeField] private Core.GameState currentState = Core.GameState.Transition;

    [Title("Settings")]
    [AssetList(Path = "/Data/GameStates/", AutoPopulate = true)]
    [SerializeField]
    private GameStateSettings[] stateSettings; // Error state must be 0 in the list

    public static event Action<Core.GameState, GameStateSettings, string> OnGameStateChange;

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

    #region Switching state method which is called by other classes

    public void SwitchState(Core.GameState state, bool overrideTransition = false)
    {
      if (_previousState == Core.GameState.Transition)
      {
        if (ReadStateSettings(currentState).interruptedByTransition == false &&
            overrideTransition == false)
        {
          Helper.Log(
            $"[GameStateDirector] Cannot change state right now. State '{currentState}' interruptedByTransition is false and overrideTransition is false.");
          return;
        }

        Helper.Log(
          "[GameStateDirector] State change with interruptedByTransition override requested.");
      }

      SwitchState(state, string.Empty);
    }

    public void SwitchState(Core.GameState state, string message)
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
      var currentStateSettings = ReadStateSettings(state);
      if (currentStateSettings == null)
      {
        state = Core.GameState.Error;
        currentStateSettings = ReadStateSettings(state);
        Helper.LogError(
          "[GameStateDirector] State settings do not exist. Error state will be triggered.");
      }

      // For trouble shooting only
      //else DebugStateSettings(stateSettings);

      // Change time scale
      UpdateTimeScale(currentStateSettings.FreezeTime);

      // Exit current game state...
      switch (currentState)
      {
        case Core.GameState.Timeline: break;
        case Core.GameState.Play: break;
        case Core.GameState.Pause: break;
        case Core.GameState.GameOver: break;
        case Core.GameState.Win: break;
        case Core.GameState.Transition: break;
        case Core.GameState.Menu: break;
        case Core.GameState.Error: break;
      }

      // Enter the new game state...
      switch (state)
      {
        case Core.GameState.Timeline: break;
        case Core.GameState.Play: break;
        case Core.GameState.Pause: break;
        case Core.GameState.GameOver: break;
        case Core.GameState.Win: break;
        case Core.GameState.Transition: break;
        case Core.GameState.Menu: break;
        case Core.GameState.Error: break;
        default:
          Helper.LogError("[GameStateDirector] An unknown state was triggered: " + state +
                          ". Switching to Error state.");
          state = Core.GameState.Error;
          break;
      }

      // Set current state to allow next change
      currentState = state;

      // Trigger state change event for anyone who's listening
      OnGameStateChange?.Invoke(state, currentStateSettings, message);
      Helper.Log("[GameStateDirector] State changed from '" + _previousState + "' to '" +
                 currentState + "' (" +
                 (message == "" ? "no message" : "message='" + message + "'") + ").");
    }

    #endregion

    private GameStateSettings ReadStateSettings(Core.GameState state)
    {
      foreach (var s in stateSettings)
      {
        if (s.Name == state) return s;
      }

      return null;
    }

    private void UpdateTimeScale(bool freezeTime)
    {
      Time.timeScale = freezeTime ? 0f : 1f;
    }

    public Core.GameState CurrentState()
    {
      return currentState;
    }

    // Only used for trouble shooting
    // ReSharper disable once UnusedMember.Local
    private void DebugStateSettings(GameStateSettings state)
    {
      foreach (var s in stateSettings)
      {
        if (s.Name != _previousState) continue;
        Helper.Log("[GameStateDirector] Settings for state '" + state.Name +
                   "' requested. Details below.");
        Helper.Log(" - Show in-game UI: " + state.ShowInGameUI + ".");
        Helper.Log(" - Freeze time: " + state.FreezeTime + ".");
        Helper.Log(" - Show in-game UI: " + state.ShowInGameUI + ".");
        Helper.Log(" - Player can move: " + state.PlayerCanMove + ".");
      }
    }
  }
}