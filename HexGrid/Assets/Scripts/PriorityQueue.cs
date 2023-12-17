using System;
using System.Collections.Generic;

public class PriorityQueue<T>
{
    private SortedDictionary<int, Queue<T>> entries = new SortedDictionary<int, Queue<T>>();

    public int Count
    {
        get
        {
            int count = 0;
            foreach (var queue in entries.Values)
            {
                count += queue.Count;
            }
            return count;
        }
    }

    public void Enqueue(T item, int priority)
    {
        if (!entries.ContainsKey(priority))
        {
            entries[priority] = new Queue<T>();
        }
        entries[priority].Enqueue(item);
    }

    public T Dequeue()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("The queue is empty");
        }

        foreach (var queue in entries.Values)
        {
            if (queue.Count > 0)
            {
                var item = queue.Dequeue();
                if (queue.Count == 0)
                {
                    entries.Remove(queue.Count);
                }
                return item;
            }
        }

        throw new InvalidOperationException("The queue is empty");
    }
}