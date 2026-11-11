using Capstone.Core.Model;

namespace Capstone.Tests;

public class LibraryQueriesTests
{
    private static Song MakeSong(
        string id,
        string artist = "Artist",
        int plays = 0,
        double minutes = 3,
        Genre genre = Genre.Rock,
        int year = 1995,
        Album? album = null,
        string? title = null) =>
        new(id, title ?? id, new Credit(artist), genre, TimeSpan.FromMinutes(minutes), year, album ?? new Album($"Album {id}", artist, year), playCount: plays);

    [Fact]
    public void Most_played_artist_combines_tracks_and_ignores_artist_case()
    {
        ITrack[] tracks =
        [
            MakeSong("one", "Other artist", plays: 8),
            MakeSong("two", "Mira", plays: 6),
            MakeSong("three", "mira", plays: 5),
            MakeSong("four", "Mira")
        ];

        Assert.Equal(new ArtistPlayTotal("Mira", 11), LibraryQueries.MostPlayedArtist(tracks));
    }

    [Fact]
    public void Most_played_artist_counts_the_main_artist_and_breaks_ties_alphabetically()
    {
        var duet = new Song("duet", "Duet", new Credit("Zeta", "Alpha"), Genre.Pop,
            TimeSpan.FromMinutes(3), 2000, new Album("Duets", "Various", 2000), playCount: 5);
        ITrack[] tracks = [duet, MakeSong("solo", "Alpha", plays: 5)];

        Assert.Equal(new ArtistPlayTotal("Alpha", 5), LibraryQueries.MostPlayedArtist(tracks));
    }

    [Fact]
    public void Artist_play_totals_can_exceed_a_single_tracks_integer_limit()
    {
        ITrack[] tracks = [MakeSong("one", plays: int.MaxValue), MakeSong("two", plays: int.MaxValue)];

        Assert.Equal(new ArtistPlayTotal("Artist", 4_294_967_294L), LibraryQueries.MostPlayedArtist(tracks));
    }

    [Fact]
    public void No_artist_is_most_played_until_a_track_has_been_played()
    {
        Assert.Null(LibraryQueries.MostPlayedArtist([MakeSong("one"), MakeSong("two")]));
    }

    [Fact]
    public void Hours_per_genre_adds_stored_duration_including_unplayed_tracks()
    {
        ITrack[] tracks =
        [
            MakeSong("pop", minutes: 45, genre: Genre.Pop),
            MakeSong("rock-one", plays: 100, minutes: 30),
            MakeSong("jazz", minutes: 90, genre: Genre.Jazz),
            MakeSong("rock-two", minutes: 15)
        ];

        var result = LibraryQueries.HoursPerGenre(tracks);

        Assert.Equal(new[] { Genre.Jazz, Genre.Rock, Genre.Pop }, result.Select(row => row.Genre));
        Assert.Equal(1.5, result[0].Hours, precision: 10);
        Assert.Equal(0.75, result[1].Hours, precision: 10);
        Assert.Equal(0.75, result[2].Hours, precision: 10);
    }

    [Fact]
    public void Unplayed_tracks_are_sorted_by_name_then_id_without_changing_the_input()
    {
        ITrack[] tracks =
        [
            MakeSong("last", title: "Zulu"),
            MakeSong("z", title: "alpha"),
            MakeSong("played", plays: 1, title: "A played song"),
            MakeSong("a", title: "Alpha")
        ];

        var result = LibraryQueries.NeverPlayedTracks(tracks);

        Assert.Equal(new[] { "a", "z", "last" }, result.Select(track => track.Id));
        Assert.Equal(new[] { "last", "z", "played", "a" }, tracks.Select(track => track.Id));
        Assert.Same(tracks[3], result[0]);
    }

    [Fact]
    public void Queries_use_current_library_membership_and_play_counts()
    {
        var library = new Library();
        library.Add(MakeSong("first", "First artist", minutes: 30));
        var tracks = library.OfType<ITrack>();
        library.Add(MakeSong("second", "Second artist", minutes: 60));

        library.Play("second");

        Assert.Equal(new ArtistPlayTotal("Second artist", 1), LibraryQueries.MostPlayedArtist(tracks));
        Assert.Equal("first", Assert.Single(LibraryQueries.NeverPlayedTracks(tracks)).Id);
        Assert.Equal(1.5, Assert.Single(LibraryQueries.HoursPerGenre(tracks)).Hours, precision: 10);

        library.Remove("second");

        Assert.Null(LibraryQueries.MostPlayedArtist(tracks));
        Assert.Equal(0.5, Assert.Single(LibraryQueries.HoursPerGenre(tracks)).Hours, precision: 10);
        Assert.Equal(new DecadeTrackCount(1990, 1), LibraryQueries.DecadeWithMostTracks(tracks));
    }

    [Fact]
    public void Longest_album_combines_its_tracks_after_individual_json_round_trips()
    {
        var album = new Album("Together", "Artist", 1995);
        ITrack[] original =
        [
            MakeSong("one", minutes: 6, album: album),
            MakeSong("two", minutes: 4, album: album),
            MakeSong("other", minutes: 8)
        ];
        var mapper = new TrackMapper();
        var restored = original
            .Select(track => Assert.IsAssignableFrom<ITrack>(mapper.FromJson(mapper.ToJson(track))))
            .ToList();

        Assert.NotSame(restored[0].Album, restored[1].Album);
        Assert.Equal(new AlbumDuration("Together", "Artist", 1995, TimeSpan.FromMinutes(10)),
            LibraryQueries.LongestAlbum(restored));
    }

    [Fact]
    public void Longest_album_does_not_count_tracks_removed_from_the_library()
    {
        var album = new Album("Formerly longest", "Artist", 1995);
        var library = new Library();
        library.Add(MakeSong("kept", minutes: 3, album: album));
        library.Add(MakeSong("removed", minutes: 60, album: album));
        library.Add(MakeSong("winner", minutes: 8));
        library.Remove("removed");

        Assert.Equal(new AlbumDuration("Album winner", "Artist", 1995, TimeSpan.FromMinutes(8)),
            LibraryQueries.LongestAlbum(library.OfType<ITrack>()));
    }

    [Fact]
    public void Albums_with_the_same_title_but_different_artists_or_years_remain_separate()
    {
        ITrack[] tracks =
        [
            MakeSong("one", minutes: 6, album: new Album("Shared title", "Artist", 1990)),
            MakeSong("two", minutes: 6, album: new Album("Shared title", "Other artist", 1990)),
            MakeSong("three", minutes: 6, album: new Album("Shared title", "Artist", 2000)),
            MakeSong("winner", minutes: 10)
        ];

        Assert.Equal(new AlbumDuration("Album winner", "Artist", 1995, TimeSpan.FromMinutes(10)),
            LibraryQueries.LongestAlbum(tracks));
    }

    [Fact]
    public void Equal_album_durations_choose_by_title_then_artist_then_year()
    {
        ITrack[] tracks =
        [
            MakeSong("one", album: new Album("Zeta", "Artist", 1980)),
            MakeSong("two", album: new Album("Alpha", "Zeta", 1980)),
            MakeSong("three", album: new Album("Alpha", "Alpha", 2000)),
            MakeSong("four", album: new Album("Alpha", "Alpha", 1990))
        ];

        Assert.Equal(new AlbumDuration("Alpha", "Alpha", 1990, TimeSpan.FromMinutes(3)),
            LibraryQueries.LongestAlbum(tracks));
    }

    [Fact]
    public void Decade_counts_tracks_by_track_year_including_both_decade_boundaries()
    {
        var compilation = new Album("Later compilation", "Various", 2024);
        ITrack[] tracks =
        [
            MakeSong("eighties", plays: 100, minutes: 120, year: 1989, album: compilation),
            MakeSong("early-nineties", year: 1990, album: compilation),
            MakeSong("late-nineties", year: 1999, album: compilation),
            MakeSong("two-thousands", year: 2000, album: compilation)
        ];

        Assert.Equal(new DecadeTrackCount(1990, 2), LibraryQueries.DecadeWithMostTracks(tracks));
    }

    [Fact]
    public void Decades_with_equal_track_counts_choose_the_earlier_decade()
    {
        ITrack[] tracks = [MakeSong("newer", year: 2000), MakeSong("older", year: 1999)];

        Assert.Equal(new DecadeTrackCount(1990, 1), LibraryQueries.DecadeWithMostTracks(tracks));
    }

    [Fact]
    public void An_empty_library_has_no_single_answers_and_empty_lists()
    {
        Assert.Null(LibraryQueries.MostPlayedArtist([]));
        Assert.Empty(LibraryQueries.HoursPerGenre([]));
        Assert.Empty(LibraryQueries.NeverPlayedTracks([]));
        Assert.Null(LibraryQueries.LongestAlbum([]));
        Assert.Null(LibraryQueries.DecadeWithMostTracks([]));
    }

    [Fact]
    public void Null_input_is_a_programming_error_for_every_query()
    {
        Assert.Throws<ArgumentNullException>(() => LibraryQueries.MostPlayedArtist(null!));
        Assert.Throws<ArgumentNullException>(() => LibraryQueries.HoursPerGenre(null!));
        Assert.Throws<ArgumentNullException>(() => LibraryQueries.NeverPlayedTracks(null!));
        Assert.Throws<ArgumentNullException>(() => LibraryQueries.LongestAlbum(null!));
        Assert.Throws<ArgumentNullException>(() => LibraryQueries.DecadeWithMostTracks(null!));
    }
}
