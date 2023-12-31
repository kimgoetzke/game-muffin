using System.Collections.Generic;
using CaptainHindsight.Core;
using CaptainHindsight.Data.GameStates;
using CaptainHindsight.Directors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.UI
{
  public class MHideUI : GameStateBehaviour
  {
    [InfoBox("Activates/deactivates the GameObject to which the script is attached based on " +
             "the GameStateSettings called 'ShowInGameUI'. Add any child(!) GameObjects you want to hide " +
             "to the list below")]
    [SerializeField]
    [ChildGameObjectsOnly]
    private List<GameObject> gameObjectsToHide = new();

    protected override void ActionGameStateChange(GameState state, GameStateSettings settings,
      string message)
    {
      if (gameObject != null && gameObjectsToHide.Count > 0)
        gameObjectsToHide.ForEach(go => go.SetActive(settings.ShowInGameUI));
    }
    
    // TODO: Remove this if no issues arise (changed on 06/08/2023)
    // private void OnEnable()
    // {
    //   GameStateDirector.OnGameStateChange += ActionGameStateChange;
    // }
    //
    // private void OnDisable()
    // {
    //   GameStateDirector.OnGameStateChange -= ActionGameStateChange;
    // }
  }
}