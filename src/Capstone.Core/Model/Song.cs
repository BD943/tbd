namespace Capstone.Core.Model;

/// <summary>A music track.</summary>
public sealed class Song : Track
{
    public Song(
        string id,
        string title,
        Credit credit,
        Genre genre,
        TimeSpan duration,
        int year,
        Album album,
        int playCount = 0,
        int? rating = null)
        : base(id, title, credit, genre, duration, year, album, playCount, rating)
    {
    }
    private string LengthLabel => Duration.TotalMinutes switch
    {
        > 8.0 => "an epic",
        < 2.0 => "a short track",
        _ => "a regular track"
    };
 
    public override string Describe() => $"Song: {DisplayName}, {LengthText}, {LengthLabel}";
}