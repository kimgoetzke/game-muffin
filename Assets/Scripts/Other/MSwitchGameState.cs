using CaptainHindsight.Core;
using CaptainHindsight.Directors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Other
{
    public class MSwitchGameState : MonoBehaviour
    {
        [InfoBox("Attach this script to the GameObject holding the timeline (e.g. SceneChange or SceneDefault). Then use Signal Emitters to call the methods contained.")]
        [InfoBox("Pick GameState from the dropdown to switch to that state using 'ToCustom()' method.")]
        [SerializeField] private GameState gameState = GameState.Error;
        
        // Only used by timeline events
        public void ToTimeline()
        {
            GameStateDirector.Instance.SwitchState(GameState.Timeline);
        }
        
        // Only used by timeline events
        public void ToPlay()
        {
            GameStateDirector.Instance.SwitchState(GameState.Play, true);
        }
        
        // Only used by timeline events
        public void ToCustom(bool withOverride = false)
        {
            GameStateDirector.Instance.SwitchState(gameState, withOverride);
        }
    }
}
