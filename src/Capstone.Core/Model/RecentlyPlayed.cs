using System.Collections;
 
namespace Capstone.Core.Model;
 
/// <summary>
/// A short memory of the last values played, newest first. Generic: T can be anything
/// </summary>
public sealed class RecentlyPlayed<T> : IEnumerable<T>
{
    private readonly List<T> items = [];   // position 0 is the newest
 
    public RecentlyPlayed(int capacity = 10)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        Capacity = capacity;
    }
 
    public int Capacity { get; }
    public int Count => items.Count;
 
    /// <summary>The value at this position, where 0 is the newest.</summary>
    public T this[int index] => items[index];
 
    /// <summary>Records a play. A value already in the list moves to the front instead of appearing twice.</summary>
    public void Add(T item)
    {
        items.RemoveAll(existing => EqualityComparer<T>.Default.Equals(existing, item));
        items.Insert(0, item);
 
        if (items.Count > Capacity)
            items.RemoveAt(items.Count - 1);   // the oldest drops out
    }
 
    /// <summary>Forgets this value. True if it was in the list.</summary>
    public bool Remove(T item) =>
        items.RemoveAll(existing => EqualityComparer<T>.Default.Equals(existing, item)) > 0;
 
    // The iterator: foreach calls this, and "yield return" hands back one value at a time.
    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in items)
            yield return item;
    }
 
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}