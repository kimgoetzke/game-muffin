namespace CaptainHindsight.Core
{
  public enum NpcType
  {
    Unspecified,
    Dinosaurs,
    Humanoid
  }

  public enum NpcIdentifier
  {
    Unspecified,
    Fukuiraptor
  }

  public enum NpcMovement
  {
    Idle,
    Wander,
    Patrol
  }

  public enum NpcBehaviour
  {
    Anxious, // Observe -> Flee
    Neutral, // Observe -> Move
    Indifferent, // Idle -> Idle
    Observant, // Observe -> Watch
    Defensive, // Observe -> Attack
    Aggressive // Investigate -> Attack
  }

  public enum NpcStateLock
  {
    Full,
    Partial,
    Off
  }

  public enum NpcAnimationTrigger
  {
    Attack,
    Unspecified
  }
}