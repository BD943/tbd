using Capstone.Core.Model;
using Capstone.Core.ViewModels;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

/// <summary>Tells the Lab 09 tests about your view model.</summary>
public sealed class Lab09Wiring : ILab09Wiring
{
    public IItemStore CreateStore() => new Library();

    // Same contract as Lab 02, so reuse its sample.
    public IItem CreateSample(int seed) => new Lab02Wiring().CreateSample(seed);

    /// <summary>A fresh view model over this store: two of them share nothing.</summary>
    public ICatalogueViewModel CreateViewModel(IItemStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        return new CatalogueViewModel(store as Library
            ?? throw new ArgumentException("This wiring only works with the Library it creates.", nameof(store)));
    }
}
