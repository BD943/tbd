using Capstone.Core.Model;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;
public sealed class Lab08Wiring : ILab08Wiring
{
    public IItemStore CreateStore() => new Library();

    // Same contract as Lab 02.
    public IItem CreateSample(int seed) => new Lab02Wiring().CreateSample(seed);

    // Same kinds as Lab 04.
    public IReadOnlyList<Type> KindTypes => new Lab04Wiring().KindTypes;

    public IItem CreateOfKind(int kindIndex, int seed) => new Lab04Wiring().CreateOfKind(kindIndex, seed);

    public async Task SaveAsync(IItemStore store, string path) => await LibraryStorage.SaveAsync(store, path);

    public async Task<IItemStore> LoadAsync(string path) => await LibraryStorage.LoadAsync(path);
}