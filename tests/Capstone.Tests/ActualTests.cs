using Capstone.Core.Model;
namespace Capstone.Tests;

public class ActualTests
{
    // ---------- Helpers ----------
 
     
    private static Song MakeTrack(Album album, int n, TimeSpan duration, int? rating = null) =>
        new($"track-{n}", $"Song {n}", new Credit("Artist"), Genre.Rock, duration, 1999, album, rating: rating);
 
    
    private static Song MakeLibraryTrack(string id, Genre genre, int year, int? rating = null) =>
        new(id, $"Song {id}", new Credit("Artist"), genre, new TimeSpan(0, 3, 0), year, new Album($"Album {id}", "Artist", year), rating: rating);
 
    private static IEnumerable<string> Ids(IEnumerable<ITrack> tracks) => tracks.Select(t => t.Id);
 
    // ---------- Albums and tracks ----------

    [Fact]
    public void Album_duration_sum_of_tracks()
    {
        var album = new Album("Album", "Artist", 1991);
        MakeTrack(album, 1, new TimeSpan(0, 3, 0));
        MakeTrack(album, 2, new TimeSpan(0, 4, 30));
        MakeTrack(album, 3, new TimeSpan(0, 2, 15));
 
        Assert.Equal(new TimeSpan(0, 9, 45), album.TotalDuration);
    }
 
    [Fact]
    public void Album_without_tracks_has_zero_duration()
    {
        var album = new Album("Empty", "Artist", 1991);
 
        Assert.Equal(TimeSpan.Zero, album.TotalDuration);
    }
 
    [Fact]
    public void Creating_track_adds_it_to_album()
    {
        var album = new Album("Album", "Artist", 1991);
        var track = MakeTrack(album, 1, new TimeSpan(0, 3, 0));
 
        Assert.Single(album.Tracks);
        Assert.Same(track, album.Tracks[0]);
    }
 
    [Fact]
    public void New_track_null_not_zero()
    {
        var album = new Album("Album", "Artist", 1991);
        var track = MakeTrack(album, 1, new TimeSpan(0, 3, 0));
 
        Assert.Null(track.Rating);
    }
 
    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void A_rating_outside_1_to_5_is_rejected(int bad)
    {
        var album = new Album("Album", "Artist", 1991);
        var track = MakeTrack(album, 1, new TimeSpan(0, 3, 0));
 
        Assert.Throws<ArgumentOutOfRangeException>(() => { track.Rating = bad; });
    }
 
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void A_rating_from_1_to_5_is_accepted(int good)
    {
        var album = new Album("Album", "Artist", 1991);
        var track = MakeTrack(album, 1, new TimeSpan(0, 3, 0));
 
        track.Rating = good;
 
        Assert.Equal(good, track.Rating);
    }
 
    [Fact]
    public void RegisterPlay_increases_play_count_by_one_each_time()
    {
        var album = new Album("Album", "Artist", 1991);
        var track = MakeTrack(album, 1, new TimeSpan(0, 3, 0));
 
        track.RegisterPlay();
        track.RegisterPlay();
        track.RegisterPlay();
 
        Assert.Equal(3, track.PlayCount);
    }
 
    [Fact]
    public void A_track_must_have_positive_duration()
    {
        var album = new Album("Album", "Artist", 1991);
 
        Assert.Throws<ArgumentOutOfRangeException>(() => MakeTrack(album, 1, TimeSpan.Zero));
    }
 
    [Fact]
    public void Display_name_shows_featured_artist_only_when_there_is_one()
    {
        var album = new Album("Album", "Artist", 1991);
        var solo = new Song("track-1", "Song", new Credit("Queen"), Genre.Rock, new TimeSpan(0, 3, 0), 1981, album);
        var duet = new Song("track-2", "Song", new Credit("Queen", "David Bowie"), Genre.Rock, new TimeSpan(0, 3, 0), 1981, album);
 
        Assert.Equal("Song — Queen", solo.DisplayName);
        Assert.Equal("Song — Queen featuring David Bowie", duet.DisplayName);
    }

   // ---------- Smart playlists ----------

    private static Song Make(string id, Genre genre, int year, int? rating = null) =>
        new(id, $"Song {id}", new Credit("Artist"), genre, new TimeSpan(0, 3, 0), year, new Album($"Album {id}", "Artist", year), rating: rating);
    [Fact]
    public void Rock_before_2000_rated_4_plus_match()
    {
        var library = new List<ITrack>
        {
            Make("a", Genre.Rock, 1975, 5),   // match
            Make("b", Genre.Rock, 1999, 4),   // match
            Make("c", Genre.Rock, 2005, 5),   // too new
            Make("d", Genre.Pop, 1990, 5),    // wrong genre
            Make("e", Genre.Rock, 1980, 3),   // rated too low
            Make("f", Genre.Rock, 1985),      // not rated
            Make("g", Genre.Rock, 2000, 4),   // too new
        };
        var playlist = new SmartPlaylist("Classic rock", new SmartPlaylistRule(Genre.Rock, 2000, 4));
 
        Assert.Equal(new[] { "a", "b" }, Ids(playlist.Evaluate(library)));
    }
 
    [Fact]
    public void Unrated_tracks_never_match_minimum_rating()
    {
        var library = new List<ITrack> { Make("a", Genre.Rock, 1990) };
        var playlist = new SmartPlaylist("Rated", new SmartPlaylistRule(MinRating: 1));
 
        Assert.Empty(playlist.Evaluate(library));
    }
 
    [Fact]
    public void Before_year_exclusive()
    {
        var library = new List<ITrack>
        {
            Make("old", Genre.Rock, 1999),
            Make("edge", Genre.Rock, 2000),
        };
        var playlist = new SmartPlaylist("Before 2000", new SmartPlaylistRule(BeforeYear: 2000));
 
        Assert.Equal(new[] { "old" }, Ids(playlist.Evaluate(library)));
    }
 
    [Fact]
    public void Minimum_rating_inclusive()
    {
        var library = new List<ITrack>
        {
            Make("four", Genre.Rock, 1990, 4),
            Make("three", Genre.Rock, 1990, 3),
        };
        var playlist = new SmartPlaylist("4+", new SmartPlaylistRule(MinRating: 4));
 
        Assert.Equal(new[] { "four" }, Ids(playlist.Evaluate(library)));
    }
 
    [Fact]
    public void A_rule_with_no_criteria_matches_every_track()
    {
        var library = new List<ITrack>
        {
            Make("a", Genre.Rock, 1970),
            Make("b", Genre.Jazz, 2020, 2),
        };
        var playlist = new SmartPlaylist("Everything", new SmartPlaylistRule());
 
        Assert.Equal(new[] { "a", "b" }, Ids(playlist.Evaluate(library)));
    }
 
    [Fact]
    public void The_playlist_evaluated_live_against_current_library()
    {
        var rated = Make("a", Genre.Rock, 1990);
        var library = new List<ITrack> { rated };
        var playlist = new SmartPlaylist("4+", new SmartPlaylistRule(MinRating: 4));
 
        Assert.Empty(playlist.Evaluate(library));
 
        rated.Rating = 5; // change an existing track
        library.Add(Make("b", Genre.Pop, 2001, 4)); // add a new track
 
        Assert.Equal(new[] { "a", "b" }, Ids(playlist.Evaluate(library)));
    }
 
    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void A_minimum_rating_outside_1_to_5_is_rejected(int bad)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SmartPlaylist("Bad", new SmartPlaylistRule(MinRating: bad)));
    }
}