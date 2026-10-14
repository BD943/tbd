namespace Capstone.Core.Model;

public sealed record SmartPlaylistRule(Genre? Genre = null, int? BeforeYear = null, int? MinRating = null)
{
    public bool Matches(ITrack track)
    {
        ArgumentNullException.ThrowIfNull(track);
 
        bool genreOk = Genre is null || track.Genre == Genre;
        bool yearOk = BeforeYear is null || track.Year < BeforeYear;
 
        // An unrated track (Rating == null) can never satisfy a minimum rating.
        bool ratingOk = MinRating is null || (track.Rating is { } stars && stars >= MinRating);
 
        return genreOk && yearOk && ratingOk;
    }
}