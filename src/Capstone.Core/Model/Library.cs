using System.Collections;
using SharpQuest.Contracts;
 
namespace Capstone.Core.Model;
 
public sealed class Library : IItemStore, IEnumerable<IItem>
{
    private readonly List<IItem> items = [];
    private readonly Dictionary<string, IItem> byId = new(StringComparer.Ordinal);
 
    public int Count => items.Count;
 
    /// <summary>The last tracks played, newest first.</summary>
    public RecentlyPlayed<Track> Recent { get; } = new();
 
    /// <summary>The item at this position, in the order added. Out of range throws ArgumentOutOfRangeException.</summary>
    public IItem this[int index] => items[index];
 
    /// <summary>The item with exactly this Id (case-sensitive), or null if there is none.</summary>
    public IItem? this[string id] => FindById(id);
 
    public void Add(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
 
        // TryAdd first: if the Id is taken, nothing has changed yet.
        if (!byId.TryAdd(item.Id, item))
            throw new ArgumentException($"An item with Id '{item.Id}' is already stored.", nameof(item));
 
        items.Add(item);
    }
 
    public IItem? FindById(string id) =>
        id is not null && byId.TryGetValue(id, out var item) ? item : null;
 
    // A copy, so later Add/Remove calls do not change a list already handed out.
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
 
    /// <summary>Plays a track: its play count goes up and it becomes the newest in Recent.</summary>
    public void Play(string id)
    {
        if (FindById(id) is not Track track)
            throw new ArgumentException($"No track with Id '{id}'.", nameof(id));
 
        track.RegisterPlay();
        Recent.Add(track);
    }
 
    // The iterator: one item at a time, in the order added.
    public IEnumerator<IItem> GetEnumerator()
    {
        foreach (var item in items)
            yield return item;
    }
 
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}