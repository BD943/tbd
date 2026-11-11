using SharpQuest.Contracts;

namespace Capstone.Core.Model;
public static class ItemQueryExtensions
{
    //A deferred query: each enumeration reads the current items.
    public static IEnumerable<string> Names(this IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return items.Select(item => item.DisplayName);
    }

    public static IReadOnlyList<IItem> Sorted(this IEnumerable<IItem> items) =>
        InNameOrder(items).ToList();

    public static IReadOnlyList<IItem> Page(this IEnumerable<IItem> items, int skip, int take)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegative(take);

        return InNameOrder(items).Skip(skip).Take(take).ToList();
    }

    //Longest display names first, with equal lengths ordered by Id.
    public static IReadOnlyList<IItem> Longest(this IEnumerable<IItem> items, int n)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        return items
            .OrderByDescending(item => item.DisplayName.Length)
            .ThenBy(item => item.Id, StringComparer.Ordinal)
            .Take(n)
            .ToList();
    }

    public static StoreStats Summary(this IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        // Read the source once so every aggregate describes the same set of items.
        var snapshot = items.ToList();
        if (snapshot.Count == 0)
            return new StoreStats(0, 0, null, 0);

        return new StoreStats(
            snapshot.Count,
            snapshot.Select(item => item.DisplayName[..1]).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            snapshot.Longest(1).Single().DisplayName,
            snapshot.Average(item => item.DisplayName.Length));
    }

    private static IOrderedEnumerable<IItem> InNameOrder(IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items
            .OrderBy(item => item.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Id, StringComparer.Ordinal);
    }
}
