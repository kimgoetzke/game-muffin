using System.Collections.Generic;
using CaptainHindsight.Core.Observer;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Other
{
    public class MInWaterTrigger : MonoBehaviour, IObservable
    {
        [ShowInInspector, ReadOnly] private List<Observer> _observers = new();
        
#pragma warning disable 0414
        [ShowInInspector, ReadOnly] private bool _isInWater;
#pragma warning restore 0414

        private void OnTriggerEnter(Collider other)
        {
            // Helper.Log($"[MInWaterTrigger] Entered: {other.tag} / {other.gameObject.name}.");
            if (!other.CompareTag("WaterRegular") && !other.CompareTag("WaterSwamp")) return;
            _isInWater = true;
            _observers.ForEach(o => Action(o, true));
        }

        private void OnTriggerExit(Collider other)
        {
            // Helper.Log($"[MInWaterTrigger] Exited: {other.tag} / {other.gameObject.name}.");
            if (!other.CompareTag("WaterRegular") && !other.CompareTag("WaterSwamp")) return;
            _isInWater = false;
            _observers.ForEach(o => Action(o, false));
        }

        private static void Action(Observer o, bool isTrue)
        {
            if (o is BoolObserver b) b.ProcessInformation(isTrue);
        }

        public void RegisterObserver(Observer observer)
        {
            _observers.Add(observer);
        }
    }
}
