using UnityEngine;

namespace CaptainHindsight.Core.StateMachine
{
  public class BStateMachine : MonoBehaviour
  {
    private BState currentState;

    private void Start()
    {
      currentState = GetInitialState();
      if (currentState != null)
        currentState.Enter();
    }

    private void Update()
    {
      if (currentState != null)
        currentState.UpdateLogic();
    }

    private void LateUpdate()
    {
      if (currentState != null)
        currentState.UpdatePhysics();
    }

    protected virtual BState GetInitialState()
    {
      return null;
    }

    public void SwitchState(BState newState)
    {
      currentState.Exit();
      currentState = newState;
      newState.Enter();
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