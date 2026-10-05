namespace Capstone.Core;

/// <summary>
/// Who performed a track. The main artist is required;
/// a featured artist is optional (null means "none").
/// </summary>
public sealed record Credit(string Artist, string? FeaturedArtist = null)
{
    public override string ToString() =>
        FeaturedArtist is null ? Artist : $"{Artist} feat. {FeaturedArtist}";
}
