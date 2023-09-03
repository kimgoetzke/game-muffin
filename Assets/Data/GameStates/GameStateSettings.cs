using CaptainHindsight.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Data.GameStates
{
    [CreateAssetMenu(fileName = "GameState-", menuName = "Scriptable Object/New game state settings")]
    public class GameStateSettings : ScriptableObject
    {
        [Title("State")]
        public GameState Name;

        [Title("UI settings")]
        public bool ShowInGameUI;

        [Title("Time settings")]
        public bool FreezeTime;

        [Title("Player settings")]
        public bool PlayerCanMove;
        public bool PlayerCanInteract;
        public bool PlayerIsAffected;

        [Title("Miscellaneous settings")] 
        [InfoBox("If true, the state will be interrupted when the TransitionManager has finished the transition and attempts to switch to GameState.Play. Set to false if this is a state that is set before a fade in to a new scene ends and you don't want the TransitionManager to interrupt it such as a GameState.Timeline or GameState.Tutorial.")]
        public bool interruptedByTransition = true;
    }
}