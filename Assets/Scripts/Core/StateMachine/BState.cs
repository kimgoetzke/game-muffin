namespace CaptainHindsight.Core.StateMachine
{
  public class BState
  {
    public string name;

    protected BStateMachine stateMachine;

    public BState(string name, BStateMachine stateMachine)
    {
      this.name = name;
      this.stateMachine = stateMachine;
    }

    public virtual void Enter()
    {
    }

    public virtual void UpdateLogic()
    {
    }

    public virtual void UpdatePhysics()
    {
    }

    public virtual void Exit()
    {
    }
  }
}