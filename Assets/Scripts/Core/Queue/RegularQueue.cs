using System.Collections.Generic;

namespace CaptainHindsight.Core.Queue
{
  public class RegularQueue<T>
  {
    private readonly Queue<T> _queue = new();

    public void Enqueue(T item)
    {
      _queue.Enqueue(item);
    }

    public T Dequeue()
    {
      if (_queue.Count == 0) Helper.LogWarning("[FifoQueue] Queue is empty.");

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