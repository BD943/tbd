namespace Capstone.Core.Model;

public sealed record ArtistPlayTotal(string Artist, long PlayCount);
public sealed record GenreHours(Genre Genre, double Hours);
public sealed record AlbumDuration(string Title, string Artist, int ReleaseYear, TimeSpan Duration);
public sealed record DecadeTrackCount(int Decade, int TrackCount);

public static class LibraryQueries
{
    /// <summary>
    /// Total plays per main artist, ignoring case. Ties use alphabetical artist order.
    /// Returns null if no track has been played.
    /// </summary>
    public static ArtistPlayTotal? MostPlayedArtist(IEnumerable<ITrack> tracks)
    {
        ArgumentNullException.ThrowIfNull(tracks);

        return tracks
            .GroupBy(track => track.Credit.Artist, StringComparer.OrdinalIgnoreCase)
            .Select(group => new ArtistPlayTotal(group.Key, group.Sum(track => (long)track.PlayCount)))
            .Where(artist => artist.PlayCount > 0)
            .OrderByDescending(artist => artist.PlayCount)
            .ThenBy(artist => artist.Artist, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }

    public static IReadOnlyList<GenreHours> HoursPerGenre(IEnumerable<ITrack> tracks)
    {
        ArgumentNullException.ThrowIfNull(tracks);

        return tracks
            .GroupBy(track => track.Genre)
            .Select(group => new GenreHours(group.Key, group.Sum(track => track.Duration.TotalHours)))
            .OrderByDescending(genre => genre.Hours)
            .ThenBy(genre => genre.Genre)
            .ToList();
    }

    //Tracks with zero plays, ordered by display name ignoring case, then by Id.
    public static IReadOnlyList<ITrack> NeverPlayedTracks(IEnumerable<ITrack> tracks)
    {
        ArgumentNullException.ThrowIfNull(tracks);

        return tracks
            .Where(track => track.PlayCount == 0)
            .OrderBy(track => track.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(track => track.Id, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// The album with the greatest total duration among the supplied tracks.
    /// Exact title, artist and release year identify an album, including after JSON round trips.
    /// Ties use ordinal title order, then artist and release year.
    /// </summary>
    public static AlbumDuration? LongestAlbum(IEnumerable<ITrack> tracks)
    {
        ArgumentNullException.ThrowIfNull(tracks);

        return tracks
            .GroupBy(track => (track.Album.Title, track.Album.Artist, track.Album.ReleaseYear))
            .Select(group => new AlbumDuration(
                group.Key.Title,
                group.Key.Artist,
                group.Key.ReleaseYear,
                new TimeSpan(group.Sum(track => track.Duration.Ticks))))
            .OrderByDescending(album => album.Duration)
            .ThenBy(album => album.Title, StringComparer.Ordinal)
            .ThenBy(album => album.Artist, StringComparer.Ordinal)
            .ThenBy(album => album.ReleaseYear)
            .FirstOrDefault();
    }
    public static DecadeTrackCount? DecadeWithMostTracks(IEnumerable<ITrack> tracks)
    {
        ArgumentNullException.ThrowIfNull(tracks);

        return tracks
            .GroupBy(track => track.Year / 10 * 10)
            .Select(group => new DecadeTrackCount(group.Key, group.Count()))
            .OrderByDescending(decade => decade.TrackCount)
            .ThenBy(decade => decade.Decade)
            .FirstOrDefault();
    }
}
