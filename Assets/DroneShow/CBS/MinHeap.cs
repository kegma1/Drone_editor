using System;
using System.Collections.Generic;
using System.Linq;

public class MinHeap<T>
{
    private readonly List<(T Item, float Priority)> heap = new();

    public int Count => heap.Count;

    // Method to check if the heap contains a specific item
    public bool Contains(T item)
    {
        return heap.Any(h => EqualityComparer<T>.Default.Equals(h.Item, item));
    }

    public void Enqueue(T item, float priority)
    {
        heap.Add((item, priority));
        HeapifyUp(heap.Count - 1);
    }

    public T Dequeue()
    {
        if (heap.Count == 0)
            throw new InvalidOperationException("Heap is empty.");

        T item = heap[0].Item;
        heap[0] = heap[^1];
        heap.RemoveAt(heap.Count - 1);
        if (heap.Count > 0)
            HeapifyDown(0);

        return item;
    }

    public (T Item, float Priority) Peek()
    {
        if (heap.Count == 0)
            throw new InvalidOperationException("Heap is empty.");

        return heap[0];
    }

    public bool TryDequeue(out T item)
    {
        if (heap.Count == 0)
        {
            item = default;
            return false;
        }

        item = Dequeue();
        return true;
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;
            if (heap[index].Priority >= heap[parent].Priority) break;
            (heap[index], heap[parent]) = (heap[parent], heap[index]);
            index = parent;
        }
    }

    private void HeapifyDown(int index)
    {
        int lastIndex = heap.Count - 1;

        while (true)
        {
            int left = 2 * index + 1;
            int right = 2 * index + 2;
            int smallest = index;

            if (left <= lastIndex && heap[left].Priority < heap[smallest].Priority)
                smallest = left;
            if (right <= lastIndex && heap[right].Priority < heap[smallest].Priority)
                smallest = right;

            if (smallest == index) break;

            (heap[index], heap[smallest]) = (heap[smallest], heap[index]);
            index = smallest;
        }
    }
}
