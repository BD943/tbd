using Capstone.Core.Model;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab07Wiring : ILab07Wiring
{
    public IItemStore CreateStore() => new Library();

    public IItem CreateSample(int seed) => new Lab02Wiring().CreateSample(seed);

    public Type ExtensionHost => typeof(ItemQueryExtensions);

    public IEnumerable<string> Names(IItemStore store) => AsLibrary(store).Names();

    public IReadOnlyList<IItem> Sorted(IEnumerable<IItem> items) => items.Sorted();

    public IReadOnlyList<IItem> Page(IEnumerable<IItem> items, int skip, int take) => items.Page(skip, take);

    public IReadOnlyList<IItem> Longest(IEnumerable<IItem> items, int n) => items.Longest(n);

    public StoreStats Summary(IEnumerable<IItem> items) => items.Summary();

    private static Library AsLibrary(IItemStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        return store as Library ?? throw new ArgumentException("This wiring only works with the Library it creates.", nameof(store));
    }
}
