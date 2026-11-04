using SharpQuest.Contracts;
namespace Capstone.Core.Model;

public sealed class AlbumTrackCounter : IDisposable
{
    private readonly Library library;
    private readonly Dictionary<Album, int> counts = [];   // albums are compared by reference
 
    public AlbumTrackCounter(Library library)
    {
        ArgumentNullException.ThrowIfNull(library);
        this.library = library;
        library.ItemAdded += OnItemAdded;   // subscribe
    }
 
    //How many tracks of this album were added to the library since this counter started listening.
    public int CountFor(Album album) => counts.GetValueOrDefault(album);
 
    private void OnItemAdded(IItem item)
    {
        if (item is not ITrack track)
            return;   // not a track, so it belongs to no album
 
        counts[track.Album] = counts.GetValueOrDefault(track.Album) + 1;
    }
 
    public void Dispose() => library.ItemAdded -= OnItemAdded;   // unsubscribe
}