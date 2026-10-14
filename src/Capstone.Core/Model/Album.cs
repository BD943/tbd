using Capstone.Core.Model;
namespace Capstone.Core.Model;

public sealed class Album
{
    private readonly List<Track> tracks = [];

    public Album(string title, string artist, int releaseYear)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(artist);

        Title = title;
        Artist = artist;
        ReleaseYear = releaseYear;
    }

    public string Title { get; }
    public string Artist { get; }
    public int ReleaseYear { get; }

    /// <summary>The tracks of this album.</summary>
    public IReadOnlyList<Track> Tracks => tracks;

    public TimeSpan TotalDuration => new(tracks.Sum(t => t.Duration.Ticks));

    // Called by Track's constructor so the parent always knows its children.
    internal void AddTrack(Track track) => tracks.Add(track);

    public override string ToString() => $"{Title} — {Artist} ({ReleaseYear})";
}