using CaptainHindsight.Core;
using CaptainHindsight.Data.GameStates;
using UnityEngine;

namespace CaptainHindsight.Directors
{
  public abstract class GameStateBehaviour : MonoBehaviour
  {
    protected abstract void ActionGameStateChange(GameState state, GameStateSettings settings,
      string message);

    protected virtual void OnEnable()
    {
      GameStateDirector.OnGameStateChange += ActionGameStateChange;
    }

    protected virtual void OnDestroy()
    {
      GameStateDirector.OnGameStateChange -= ActionGameStateChange;
    }
  }
}