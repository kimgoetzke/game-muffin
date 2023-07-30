using CaptainHindsight.Core;

namespace CaptainHindsight.Quests
{
  public interface IQuestCollectable
  {
    CollectableQuestIdentifier CollectableIdentifier { get; }

    void Collect();
  }
}