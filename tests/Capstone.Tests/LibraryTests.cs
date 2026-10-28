using Capstone.Core.Model;
 
namespace Capstone.Tests;
 
public class LibraryTests
{
    private static Song MakeSong(int n) =>
        new($"song-{n:D3}", $"Song {n}", new Credit("Artist"), Genre.Rock, new TimeSpan(0, 3, 0), 2000, new Album("Album", "Artist", 2000));
 
    private static Library LibraryOf(int count)
    {
        var library = new Library();
        for (int n = 1; n <= count; n++)
            library.Add(MakeSong(n));
        return library;
    }
 
    //Indexers 
 
    [Fact]
    public void Indexer_by_position_follows_order_items_were_added()
    {
        var library = LibraryOf(3);
 
        Assert.Equal("song-001", library[0].Id);
        Assert.Equal("song-003", library[2].Id);
    }
 
    [Fact]
    public void Indexer_by_position_out_of_range_throws()
    {
        var library = LibraryOf(3);
 
        Assert.Throws<ArgumentOutOfRangeException>(() => { _ = library[3]; });
    }
 
    [Fact]
    public void Indexer_by_id_finds_item_with_that_id()
    {
        var library = LibraryOf(42);
 
        var found = Assert.IsType<Song>(library["song-042"]);
 
        Assert.Equal("Song 42", found.Title);
    }
 
    [Fact]
    public void Indexer_by_id_with_unknown_id_gives_null()
    {
        var library = LibraryOf(3);
 
        Assert.Null(library["song-999"]);
    }
 
    //Iterator
 
    [Fact]
    public void Foreach_visits_every_item_in_order_added()
    {
        var library = LibraryOf(3);
        var seen = new List<string>();
 
        foreach (var item in library)
            seen.Add(item.Id);
 
        Assert.Equal(new[] { "song-001", "song-002", "song-003" }, seen);
    }
 
    //Store rules still hold
 
    [Fact]
    public void Adding_a_duplicate_id_throws_and_changes_nothing()
    {
        var library = LibraryOf(2);
 
        Assert.Throws<ArgumentException>(() => library.Add(MakeSong(1)));
        Assert.Equal(2, library.Count);
    }
 
    //Playing and the generic "recently played"
 
    [Fact]
    public void Playing_a_track_counts_the_play_and_remembers_it()
    {
        var library = LibraryOf(2);
 
        library.Play("song-001");
        library.Play("song-002");
        library.Play("song-001");
 
        Assert.Equal(2, Assert.IsType<Song>(library["song-001"]).PlayCount);
        Assert.Equal(new[] { "song-001", "song-002" }, library.Recent.Select(t => t.Id));
    }
 
    [Fact]
    public void Removing_a_track_also_removes_it_from_recently_played()
    {
        var library = LibraryOf(2);
        library.Play("song-001");
 
        library.Remove("song-001");
 
        Assert.Empty(library.Recent);
    }
 
    [Fact]
    public void Recently_played_keeps_newest_first_and_forgets_oldest()
    {
        var recent = new RecentlyPlayed<Song>(2);
 
        recent.Add(MakeSong(1));
        recent.Add(MakeSong(2));
        recent.Add(MakeSong(3));
 
        Assert.Equal(new[] { "song-003", "song-002" }, recent.Select(t => t.Id));
    }
 
    [Fact]
    public void Playing_something_again_moves_it_to_the_front_without_a_duplicate()
    {
        var recent = new RecentlyPlayed<Song>(5);
        var first = MakeSong(1);
 
        recent.Add(first);
        recent.Add(MakeSong(2));
        recent.Add(first);
 
        Assert.Equal(2, recent.Count);
        Assert.Equal(new[] { "song-001", "song-002" }, recent.Select(t => t.Id));
    }
 
    [Fact]
    public void Recently_played_is_generic_and_works_for_numbers_and_words_too()
    {
        var numbers = new RecentlyPlayed<int>(2);
        numbers.Add(1);
        numbers.Add(2);
        numbers.Add(3);
 
        var words = new RecentlyPlayed<string>(2);
        words.Add("a");
        words.Add("b");
        words.Add("a");
 
        Assert.Equal(new[] { 3, 2 }, numbers);
        Assert.Equal(new[] { "a", "b" }, words);
    }
 
    [Fact]
    public void A_capacity_below_1_is_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RecentlyPlayed<int>(0));
    }
}