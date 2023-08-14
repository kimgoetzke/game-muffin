using System.Collections.Generic;
using CaptainHindsight.Core.Observer;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight.Core.StateMachine
{
  public class BStateMachine : MonoBehaviour, IObservable
  {
    private BState _currentState;
    
    [FoldoutGroup("Observers", true)] [ShowInInspector] [ReadOnly]
    private List<Observer.Observer> _observers = new();

    private void Start()
    {
      _currentState = GetInitialState();
      _currentState?.Enter();
    }

    private void Update()
    {
      _currentState?.UpdateLogic();
    }

    private void LateUpdate()
    {
      _currentState?.UpdatePhysics();
    }

    protected virtual BState GetInitialState()
    {
      return null;
    }

    public void SwitchState(BState newState)
    {
      _currentState.Exit();
      _currentState = newState;
      foreach (var observer in _observers)
      {
        if (observer is NpcObserver npcObserver) npcObserver.ProcessInformation(newState);
      }
      newState.Enter();
    }
    
    public void RegisterObserver(Observer.Observer observer)
    {
      _observers.Add(observer);
    }

    //private void OnGUI()
    //{
    //    GUILayout.BeginArea(new Rect(10f, 10f, 200f, 100f));
    //    string content = currentState != null ? currentState.name : "(no current state)";
    //    GUILayout.Label($"<color='white'><size=40>{content}</size></color>");
    //    GUILayout.EndArea();
    //}
  }
}