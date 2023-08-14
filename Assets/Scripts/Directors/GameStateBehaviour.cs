
using CaptainHindsight.Data.GameStates;
using UnityEngine;

namespace CaptainHindsight.Directors
{
  public abstract class GameStateBehaviour : MonoBehaviour
  {
    protected abstract void ActionGameStateChange(Core.GameState state, GameStateSettings settings,
      string message);

    protected virtual void Awake()
    {
      GameStateDirector.OnGameStateChange += ActionGameStateChange;
    }

    protected virtual void OnDestroy()
    {
      GameStateDirector.OnGameStateChange -= ActionGameStateChange;
    }
  }
}