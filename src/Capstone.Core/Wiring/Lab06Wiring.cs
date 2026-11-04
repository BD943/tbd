using Capstone.Core.Model;
using SharpQuest.Contracts;
namespace Capstone.Core.Wiring;

public sealed class Lab06Wiring : ILab06Wiring
{
    public IItemStore CreateStore() => new Library();
 
    public IItem CreateSample(int seed) => new Lab02Wiring().CreateSample(seed);
 
    //The type that declares the ItemAdded event.
    public Type PublisherType => typeof(Library);
 
    //Subscribes the handler to ItemAdded and returns the function that unsubscribes exactly this one.
    public Action Subscribe(IItemStore store, Action<IItem> handler)
    {
        var library = AsLibrary(store);
        ArgumentNullException.ThrowIfNull(handler);
 
        library.ItemAdded += handler;
        return () => library.ItemAdded -= handler;
    }
 
    public IReadOnlyList<IItem> Where(IItemStore store, Func<IItem, bool> keep) => AsLibrary(store).Filter(keep);
 
    public IReadOnlyList<string> Map(IItemStore store, Func<IItem, string> describe) => AsLibrary(store).Map(describe);
 
    public void ForEach(IItemStore store, Action<IItem> action) => AsLibrary(store).ForEach(action);
 
    private static Library AsLibrary(IItemStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        return store as Library ?? throw new ArgumentException("This wiring only works with the Library it creates.", nameof(store));
    }
}