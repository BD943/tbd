using Capstone.Core.Model;
using SharpQuest.Contracts;
 
namespace Capstone.Core.Wiring;
 
/// <summary>Tells the Lab 04 tests about the Track family: its base, its kinds, and what they can do.</summary>
public sealed class Lab04Wiring : ILab04Wiring
{
    public IItemStore CreateStore() => new Library();
 
    /// <summary>The abstract base class of the hierarchy.</summary>
    public Type BaseType => typeof(Track);
 
    /// <summary>The concrete kinds. Their position is the kindIndex used by CreateOfKind.</summary>
    public IReadOnlyList<Type> KindTypes { get; } = [typeof(Song), typeof(PodcastEpisode)];
 
    /// <summary>
    /// A valid item of KindTypes[kindIndex]. Same (kind, seed) gives the same Id.
    /// The Id contains the kind, so different kinds never share an Id.
    /// </summary>
    public IItem CreateOfKind(int kindIndex, int seed)
    {
        var album = new Album($"Sample Album {seed}", "Sample Artist", 2000);
 
        Track item = kindIndex switch
        {
            0 => new Song($"song-{seed:D3}", $"Sample Song {seed}", new Credit("Sample Artist"),
                          Genre.Rock, TimeSpan.FromSeconds(200), 2000, album),
            1 => new PodcastEpisode($"podcast-{seed:D3}", $"Sample Episode {seed}", new Credit("Sample Host"),
                                    Genre.Pop, TimeSpan.FromSeconds(1800), 2000, album, episodeNumber: seed),
            _ => throw new ArgumentOutOfRangeException(nameof(kindIndex), $"There is no kind number {kindIndex}.")
        };
 
        return item;
    }
 
    /// <summary>Asks the item to describe itself through the base type; the override of its kind answers.</summary>
    public string Describe(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
 
        return item is Track track
            ? track.Describe()
            : throw new ArgumentException($"{item.GetType().Name} is not a Track.", nameof(item));
    }
 
    /// <summary>One description per track in the store, in store order, from one loop that doesn't know the kinds.</summary>
    public IReadOnlyList<string> DescribeAll(IItemStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
 
        return LibraryReport.DescribeAll(store.All().OfType<ITrack>());
    }
 
    /// <summary>A short label for what the item is. The _ arm catches anything that is not one of our kinds.</summary>
    public string Classify(IItem item) => item switch
    {
        null => throw new ArgumentNullException(nameof(item)),
        Song => "song",
        PodcastEpisode => "podcast episode",
        _ => "unknown item"
    };
 
    /// <summary>Finds an item whose DisplayName is exactly this name (case-sensitive). A null or blank name is just not found.</summary>
    public bool TryFindByName(IItemStore store, string name, out IItem? found)
    {
        found = null;
        ArgumentNullException.ThrowIfNull(store);
 
        if (string.IsNullOrWhiteSpace(name))
            return false;
 
        foreach (var item in store.All())
        {
            if (string.Equals(item.DisplayName, name, StringComparison.Ordinal))
            {
                found = item;
                return true;
            }
        }
 
        return false;
    }
 
    /// <summary>Adds every item in order and returns how many were added. The store's own rules apply to each one.</summary>
    public int AddAll(IItemStore store, params IItem[] items)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(items);
 
        int added = 0;
        foreach (var item in items)
        {
            store.Add(item);   // throws for null or a duplicate Id; what was added before stays added
            added++;
        }
 
        return added;
    }
}