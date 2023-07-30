using System.Collections.Generic;

namespace CaptainHindsight.Core.Queue
{
  public class UniqueQueue<T>
  {
    private readonly Queue<T> queue;

    public UniqueQueue()
    {
      queue = new Queue<T>();
    }

    public void Enqueue(T item)
    {
      if (!queue.Contains(item))
      {
        queue.Enqueue(item);
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
      if (queue.Count == 0) Helper.LogWarning("[UniqueQueue] Queue is empty.");

      return queue.Dequeue();
    }

    public bool IsEmpty()
    {
      return queue.Count == 0;
    }

    public int Count()
    {
      return queue.Count;
    }

    public void Clear()
    {
      queue.Clear();
    }
  }
}