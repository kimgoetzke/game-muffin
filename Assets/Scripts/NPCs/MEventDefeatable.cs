using CaptainHindsight.Core;
using CaptainHindsight.Core.Observer;
using CaptainHindsight.Managers;
using Sirenix.OdinInspector;

namespace CaptainHindsight.NPCs
{
  public class MEventDefeatable : Observer
  {
    [Title("Configuration")]
    [InfoBox(
      "This script must be placed on the same object as the HealthManager. It will automatically get the reference and use it to identify who defeated this NPC.")]
    [ShowInInspector]
    [Required]
    private NpcIdentifier _identifier;

    [ShowInInspector] [Required] private string _eventId;

    public override void ProcessInformation()
    {
      EventManager.Instance.TriggerWaveEvent(_eventId, _identifier);
    }

    public void SetEventIdentifier(string id)
    {
      _eventId = id;
    }

    public void SetNpcIdentifier(NpcIdentifier npcIdentifier)
    {
      _identifier = npcIdentifier;
    }
  }
}