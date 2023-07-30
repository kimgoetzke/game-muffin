using CaptainHindsight.Core;
using CaptainHindsight.Core.Observer;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Directors
{
  public class MGameStateOnDeath : Observer
  {
    [SerializeField] [Required] private string message;

    public override void ProcessInformation()
    {
      if (message == "") Helper.LogWarning("[MGameStateOnDeath] You forgot to set the message.");
      GameStateDirector.Instance.SwitchState(GameState.GameOver, message);
    }
  }
}