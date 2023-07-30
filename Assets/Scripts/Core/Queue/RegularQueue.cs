using System.Collections.Generic;

namespace CaptainHindsight.Core.Queue
{
  public class RegularQueue<T>
  {
    private Queue<T> queue;

    public RegularQueue()
    {
      queue = new Queue<T>();
    }

    public void Enqueue(T item)
    {
      queue.Enqueue(item);
    }

    public T Dequeue()
    {
      if (queue.Count == 0) Helper.LogWarning("[FifoQueue] Queue is empty.");

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