using System.Threading.Tasks;
using CaptainHindsight.Core;
using UnityEngine;

namespace CaptainHindsight.NPCs
{
  public class MRemoteSetup : MonoBehaviour
  {
    private Npc _npc;

    private void Awake()
    {
      _npc = Helper.GetComponentSafely<Npc>(gameObject);
    }

    public async void SetCooperationTarget(Transform target)
    {
      // Note: This delay is necessary as it takes a few milliseconds before 
      // the NPC script has initialised all states. If you remove the delay,
      // you'll get a NullReferenceException, however you can reduce it.
      await Task.Delay(System.TimeSpan.FromSeconds(1f));
      _npc.CooperateWithOthers(target);
    }
  }
}