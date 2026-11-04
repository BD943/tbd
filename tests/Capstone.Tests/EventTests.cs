using Capstone.Core.Model;
using SharpQuest.Contracts; 
namespace Capstone.Tests;
 
public class EventTests
{
    private static Song MakeSong(int n, Album? album = null) =>
        new($"song-{n:D3}", $"Song {n}", new Credit("Artist"), Genre.Rock, new TimeSpan(0, 3, 0), 2000, album ?? new Album("Album", "Artist", 2000));
 
    private static Library LibraryOf(int count)
    {
        var library = new Library();
        for (int n = 1; n <= count; n++)
            library.Add(MakeSong(n));
        return library;
    }
 
    //The ItemAdded event
 
    [Fact]
    public void Adding_an_item_raises_ItemAdded_once_with_that_item()
    {
        var library = new Library();
        var heard = new List<IItem>();
        library.ItemAdded += heard.Add;
        var song = MakeSong(1);
 
        library.Add(song);
 
        Assert.Same(song, Assert.Single(heard));
    }
 
    [Fact]
    public void ItemAdded_is_raised_after_the_item_is_in_the_library()
    {
        var library = new Library();
        int countSeen = -1;
        bool foundSeen = false;
        library.ItemAdded += item =>
        {
            countSeen = library.Count;
            foundSeen = library.FindById(item.Id) is not null;
        };
 
        library.Add(MakeSong(1));
 
        Assert.Equal(1, countSeen);
        Assert.True(foundSeen, "The handler ran before the item was stored.");
    }
 
    [Fact]
    public void A_duplicate_id_raises_nothing()
    {
        var library = LibraryOf(1);
        int raised = 0;
        library.ItemAdded += _ => raised++;
 
        Assert.Throws<ArgumentException>(() => library.Add(MakeSong(1)));
 
        Assert.Equal(0, raised);
    }
 
    [Fact]
    public void A_null_item_raises_nothing()
    {
        var library = new Library();
        int raised = 0;
        library.ItemAdded += _ => raised++;
 
        Assert.Throws<ArgumentNullException>(() => library.Add(null!));
 
        Assert.Equal(0, raised);
    }
 
    [Fact]
    public void Adding_with_nobody_subscribed_does_not_throw()
    {
        var library = new Library();
 
        library.Add(MakeSong(1));
 
        Assert.Equal(1, library.Count);
    }
 
    [Fact]
    public void The_same_handler_subscribed_twice_is_called_twice_and_unsubscribing_once_leaves_one()
    {
        var library = new Library();
        int calls = 0;
        Action<IItem> handler = _ => calls++;
        library.ItemAdded += handler;
        library.ItemAdded += handler;
 
        library.Add(MakeSong(1));
        Assert.Equal(2, calls);
 
        library.ItemAdded -= handler;
        library.Add(MakeSong(2));
        Assert.Equal(3, calls);
    }
 
    //Two subscribers 
 
    [Fact]
    public void The_log_and_the_album_counter_both_hear_every_addition()
    {
        var library = new Library();
        var album = new Album("Album", "Artist", 2000);
        using var log = new LibraryLog(library);
        using var counter = new AlbumTrackCounter(library);
 
        library.Add(MakeSong(1, album));
        library.Add(MakeSong(2, album));
        library.Add(MakeSong(3, new Album("Other", "Other", 1990)));
 
        Assert.Equal(3, log.Lines.Count);
        Assert.Equal(2, counter.CountFor(album));
    }
 
    [Fact]
    public void A_disposed_log_stops_listening()
    {
        var library = new Library();
        var log = new LibraryLog(library);
        library.Add(MakeSong(1));
 
        log.Dispose();
        library.Add(MakeSong(2));
 
        Assert.Single(log.Lines);
    }
 
    //Functions as parameters
 
    [Fact]
    public void Filter_keeps_what_the_function_accepts_and_calls_it_once_per_item()
    {
        var library = LibraryOf(3);
        int calls = 0;
 
        var kept = library.Filter(item =>
        {
            calls++;
            return item.Id != "song-002";
        });
 
        Assert.Equal(new[] { "song-001", "song-003" }, kept.Select(i => i.Id));
        Assert.Equal(3, calls);
    }
 
    [Fact]
    public void Map_turns_every_item_into_a_result_of_any_type()
    {
        var library = LibraryOf(2);
 
        var ids = library.Map(item => item.Id);
        var lengths = library.Map(item => item.DisplayName.Length);
 
        Assert.Equal(new[] { "song-001", "song-002" }, ids);
        Assert.Equal(2, lengths.Count);
    }
 
    [Fact]
    public void ForEach_runs_the_action_on_every_item_in_order()
    {
        var library = LibraryOf(3);
        var seen = new List<string>();
 
        library.ForEach(item => seen.Add(item.Id));
 
        Assert.Equal(new[] { "song-001", "song-002", "song-003" }, seen);
    }
 
    [Fact]
    public void A_null_function_is_a_programming_error()
    {
        var library = LibraryOf(1);
 
        Assert.Throws<ArgumentNullException>(() => { library.Filter(null!); });
        Assert.Throws<ArgumentNullException>(() => { library.Map<string>(null!); });
        Assert.Throws<ArgumentNullException>(() => { library.ForEach(null!); });
    }
}