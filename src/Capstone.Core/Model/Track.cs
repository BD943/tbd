namespace Capstone.Core.Model;

/// <summary>
/// The abstract base of every kind of audio in the library.
/// </summary>
public abstract class Track : ITrack
{
    private int? rating;
 
    protected Track(
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
 
    /// <summary>1–5 stars. null means "not rated yet", which is not the same as 0.</summary>
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
 
    /// <summary>Every kind must describe itself. There is no default.</summary>
    public abstract string Describe();
 
    /// <summary>The length as minutes:seconds. Minutes keep counting past 59, so 75 minutes is "75:00".</summary>
    protected string LengthText => $"{(int)Duration.TotalMinutes}:{Duration.Seconds:D2}";
 
    public override string ToString() =>
        $"{DisplayName} ({Year}, {Genre}, {LengthText}, " +  $"{(Rating is { } r ? $"{r}/5" : "not rated yet")})";
}
