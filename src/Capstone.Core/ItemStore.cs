using SharpQuest.Contracts;

namespace Capstone.Core;

/// <summary>Holds items in insertion order and finds them by exact (case-sensitive) Id.</summary>
public sealed class ItemStore : IItemStore
{
    private readonly List<IItem> items = [];
    private readonly Dictionary<string, IItem> byId = new(StringComparer.Ordinal);

    public int Count => items.Count;

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
        if (id is null || !byId.Remove(id))
            return false;

        items.RemoveAll(i => string.Equals(i.Id, id, StringComparison.Ordinal));
        return true;
    }
}