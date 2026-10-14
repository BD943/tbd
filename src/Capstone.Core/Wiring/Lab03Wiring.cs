using Capstone.Core.Model;
using SharpQuest.Contracts;
 
namespace Capstone.Core.Wiring;
 
/// <summary>Tells the Lab 03 tests how to search this capstone.</summary>
public sealed class Lab03Wiring : ILab03Wiring
{
    public IItemStore CreateStore() => new ItemStore();
 
    
    public IItem CreateSample(int seed) => new Lab02Wiring().CreateSample(seed);
 
    public IReadOnlyList<IItem> Search(IItemStore store, string query)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(query);
 
        var matcher = SearchMatcher.Create();
        return [.. store.All().Where(item => matcher.Matches(item.DisplayName, query))];
    }
}