using System.Collections;
using SharpQuest.Contracts;

namespace Capstone.Core.Model;
public sealed class Library : IItemStore, IEnumerable<IItem>
{
    private readonly List<IItem> items = [];
    private readonly Dictionary<string, IItem> byId = new(StringComparer.Ordinal);

    public int Count => items.Count;

    //last tracks played, newest first.
    public RecentlyPlayed<Track> Recent { get; } = new();

    public event Action<IItem>? ItemAdded;

    //The item at this position, in the order added. Out of range throws ArgumentOutOfRangeException.
    public IItem this[int index] => items[index];

    //The item with exactly this Id (case-sensitive), or null if there is none.
    public IItem? this[string id] => FindById(id);

    public void Add(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        // TryAdd first: if the Id is taken, nothing has changed yet, and nothing is announced.
        if (!byId.TryAdd(item.Id, item))
            throw new ArgumentException($"An item with Id '{item.Id}' is already stored.", nameof(item));

        items.Add(item);

        ItemAdded?.Invoke(item);   // announce only once the item is really in
    }

    public IItem? FindById(string id) =>
        id is not null && byId.TryGetValue(id, out var item) ? item : null;

    public IReadOnlyList<IItem> All() => [.. items];

    public bool Remove(string id)
    {
        if (id is null || !byId.Remove(id, out var removed))
            return false;

        items.Remove(removed);
        if (removed is Track track)
            Recent.Remove(track);   // a removed track must not linger in "recently played"
        return true;
    }

    //Plays a track: its play count goes up and it becomes the newest in Recent.
    public void Play(string id)
    {
        if (FindById(id) is not Track track)
            throw new ArgumentException($"No track with Id '{id}'.", nameof(id));

        track.RegisterPlay();
        Recent.Add(track);
    }

    /// <summary>The items the function says yes to, in library order. The function is called once per item.</summary>
    public IReadOnlyList<IItem> Filter(Func<IItem, bool> keep)
    {
        ArgumentNullException.ThrowIfNull(keep);

        var result = new List<IItem>();
        foreach (var item in items)
        {
            if (keep(item))
                result.Add(item);
        }
        return result;
    }

    /// <summary>One result per item, in library order, each produced by the function. Works for any result type.</summary>
    public IReadOnlyList<TResult> Map<TResult>(Func<IItem, TResult> convert)
    {
        ArgumentNullException.ThrowIfNull(convert);

        var result = new List<TResult>();
        foreach (var item in items)
            result.Add(convert(item));
        return result;
    }

    //Runs the action on every item, in library order.
    public void ForEach(Action<IItem> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in items)
            action(item);
    }

    // The iterator: one item at a time, in the order added.
    public IEnumerator<IItem> GetEnumerator()
    {
        foreach (var item in items)
            yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}