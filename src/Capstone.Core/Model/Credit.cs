using Capstone.Core.Model;
namespace Capstone.Core.Model;
// Who performed a track. The main artist is required;
 
public sealed record Credit(string Artist, string? FeaturedArtist = null)
{
    public override string ToString() =>
        FeaturedArtist is null ? Artist : $"{Artist} featuring {FeaturedArtist}";
}
