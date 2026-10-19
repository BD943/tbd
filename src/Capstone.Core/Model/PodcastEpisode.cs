namespace Capstone.Core.Model;
 
/// <summary>One episode of a podcast. The "album" is the show it belongs to.</summary>
public sealed class PodcastEpisode : Track
{
    public PodcastEpisode(
        string id,
        string title,
        Credit credit,
        Genre genre,
        TimeSpan duration,
        int year,
        Album album,
        int episodeNumber,
        int playCount = 0,
        int? rating = null)
        : base(id, title, credit, genre, duration, year, album, playCount, rating)
    {
        EpisodeNumber = episodeNumber;
    }
 
    public int EpisodeNumber { get; }
 
    private string LengthLabel => Duration.TotalMinutes switch
    {
        > 60.0 => "a marathon",
        < 15.0 => "a quick listen",
        _ => "a full episode"
    };
 
    public override string Describe() => $"Podcast #{EpisodeNumber}: {DisplayName}, {LengthText}, {LengthLabel}";
}