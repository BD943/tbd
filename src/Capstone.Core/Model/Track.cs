using SharpQuest.Contracts;
using Capstone.Core.Model;
namespace Capstone.Core.Model;

public sealed class Track : IItem
{
    private int? rating;

    public Track(
        string id,
        string title,
        Credit credit,
        Genre genre,
        TimeSpan duration,
        int year,
        Album album,
        int playCount = 0,
        int? rating = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(credit);
        ArgumentNullException.ThrowIfNull(album);
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        ArgumentOutOfRangeException.ThrowIfNegative(playCount);

        Id = id;
        Title = title;
        Credit = credit;
        Genre = genre;
        Duration = duration;
        Year = year;
        Album = album;
        PlayCount = playCount;
        Rating = rating; // goes through the validating setter

        album.AddTrack(this);
    }

    public string Id { get; }
    public string Title { get; }
    public Credit Credit { get; }
    public Genre Genre { get; }
    public TimeSpan Duration { get; }
    public int Year { get; }
    public Album Album { get; }
    public int PlayCount { get; private set; }
    public string DisplayName => $"{Title} — {Credit}";

    /// <summary>1–5 stars. null means "not rated yet".</summary>
    public int? Rating
    {
        get => rating;
        set
        {
            if (value is < 1 or > 5)
                throw new ArgumentOutOfRangeException(nameof(value), "Rating must be 1–5, or null.");
            rating = value;
        }
    }

    public void RegisterPlay() => PlayCount++;

    public override string ToString() =>
        $"{DisplayName} ({Year}, {Genre}, {Duration:m\\:ss}, " + $"{(Rating is { } r ? $"{r}/5" : "not rated yet")})";
}
