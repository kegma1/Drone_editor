using System;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<TElement, TPriority>
{
    private List<(TElement Element, TPriority Priority)> _elements = new();

    public void Enqueue(TElement element, TPriority priority)
    {
        _elements.Add((element, priority));
        _elements.Sort((a, b) => Comparer<TPriority>.Default.Compare(a.Priority, b.Priority));
    }

    public (TElement Element, TPriority Priority) Dequeue()
    {
        var item = _elements[0];
        _elements.RemoveAt(0);
        return item;
    }

    public int Count => _elements.Count;
}
