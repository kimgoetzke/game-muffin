namespace CaptainHindsight.Core
{
  public enum ActionType
  {
    Damage,
    Health,
    Positive,
    Negative,
    Other
  }

  public enum MessageType
  {
    Message,
    Countdown
  }

  public enum GameState
  {
    Timeline,
    Play,
    Pause,
    GameOver,
    Win,
    Transition,
    Menu,
    Error
  }

  public enum CollectableQuestIdentifier
  {
    // Intended to be usef by quest system to mark items as collectables
  }
}