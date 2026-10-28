using Capstone.Core.Model;
using SharpQuest.Contracts;
 
namespace Capstone.Core.Wiring;
 

public sealed class Lab05Wiring : ILab05Wiring
{
    public IItemStore CreateStore() => new Library();
 
    
    public IItem CreateSample(int seed) => new Lab02Wiring().CreateSample(seed);
 
    public Type CollectionType => typeof(Library);
 
    /// <summary>The open generic definition: RecentlyPlayed with its type parameter left empty.</summary>
    public Type GenericDefinition => typeof(RecentlyPlayed<>);
 
    /// <summary>Goes through the this[int] indexer. Out of range throws ArgumentOutOfRangeException.</summary>
    public IItem At(IItemStore store, int index) => AsLibrary(store)[index];
 
    /// <summary>Goes through the this[string] indexer, which gives null for an unknown Id.</summary>
    public IItem? ById(IItemStore store, string id) => AsLibrary(store)[id];
 
    /// <summary>foreach over the library itself, not over All().</summary>
    public IReadOnlyList<IItem> EachInOrder(IItemStore store)
    {
        var result = new List<IItem>();
        foreach (var item in AsLibrary(store))
            result.Add(item);
        return result;
    }
 
    /// <summary>
    /// An iterator: nothing runs until somebody enumerates the result,
    /// so even the argument checks happen at that moment.
    /// </summary>
    public IEnumerable<IItem> Matching(IItemStore store, string text)
    {
        var library = AsLibrary(store);
        ArgumentNullException.ThrowIfNull(text);
 
        foreach (var item in library)
        {
            if (item.DisplayName.Contains(text, StringComparison.OrdinalIgnoreCase))
                yield return item;
        }
    }
 
    /// <summary>Pushes each value into a RecentlyPlayed&lt;T&gt; and returns what it holds, newest first.</summary>
    public IReadOnlyList<T> Recent<T>(int capacity, params T[] pushed)
    {
        ArgumentNullException.ThrowIfNull(pushed);
 
        var recent = new RecentlyPlayed<T>(capacity);   // a capacity below 1 throws here
        foreach (var value in pushed)
            recent.Add(value);
 
        return [.. recent];
    }
 
    private static Library AsLibrary(IItemStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        return store as Library ?? throw new ArgumentException("This wiring only works with the Library it creates.", nameof(store));
    }
}