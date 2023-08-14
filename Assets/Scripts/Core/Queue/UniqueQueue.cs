using System.Collections.Generic;

namespace CaptainHindsight.Core.Queue
{
  public class UniqueQueue<T>
  {
    private readonly Queue<T> _queue = new();

    public void Enqueue(T item)
    {
      if (!_queue.Contains(item))
      {
        _queue.Enqueue(item);
        Helper.Log($"[UniqueQueue] Enqueueing item {item} with hashcode: {item.GetHashCode()}.");
      }
      else
      {
        Helper.LogWarning(
          $"[UniqueQueue] Item already exists in queue: {item} with hashcode: {item.GetHashCode()}.");
      }
    }

    public T Dequeue()
    {
      if (_queue.Count == 0) Helper.LogWarning("[UniqueQueue] Queue is empty.");

      return _queue.Dequeue();
    }

    public bool IsEmpty()
    {
      return _queue.Count == 0;
    }

    public int Count()
    {
      return _queue.Count;
    }

    public void Clear()
    {
      _queue.Clear();
    }
  }
}