using System;
using System.Globalization;
using System.Threading;
using CaptainHindsight.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

namespace CaptainHindsight.UI
{
    public class MSkipCutScene : MonoBehaviour
    {
        [SerializeField, Required] private GameObject instructions;
        private PlayerInputActions _playerInputActions;
        private CancellationTokenSource _tokenSource;
        private bool _canSkipNow;
        [SerializeField, Required] private PlayableDirector playableDirector;
        [SerializeField, Min(0.5f)] private float skipToSecond;

        private void Continue(InputAction.CallbackContext context)
        {
            if (_canSkipNow == false)
            {
                instructions.gameObject.SetActive(true);
                _canSkipNow = true;
                return;
            }
            
            _tokenSource?.Cancel();
            _tokenSource = new CancellationTokenSource();
            Helper.Log($"[MSkipCutScene] Skipped cutscene (from {playableDirector.time} to {skipToSecond}).");
            playableDirector.time = skipToSecond;
        }

        private void OnEnable()
        {
            instructions.gameObject.SetActive(false);
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Menu.Enable();
            _playerInputActions.Menu.Continue.performed += Continue;
        }

        private void OnDisable()
        {
            instructions.gameObject.SetActive(false);
            _playerInputActions.Menu.Continue.performed -= Continue;
            _playerInputActions.Menu.Disable();
        }
    }
}
