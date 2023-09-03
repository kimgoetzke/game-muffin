using System.Threading.Tasks;
using CaptainHindsight.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.NPCs
{
  public class MRemoteSetup : MonoBehaviour
  {
    [InfoBox(
      "This delay is necessary as it takes a few milliseconds before the NPC script has initialised all states. If you remove the delay, you'll get a NullReferenceException, however you can reduce below 1.")]
    [SerializeField]
    private float delay = 1f;

    private Npc _npc;

    private void Awake()
    {
      _npc = gameObject.GetComponentSafely<Npc>();
    }

    public async void SetCooperationTarget(Transform target)
    {
      await Task.Delay(System.TimeSpan.FromSeconds(delay));
      _npc.CooperateWithOthers(target);
    }
  }
}