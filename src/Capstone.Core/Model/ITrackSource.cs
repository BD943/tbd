using Capstone.Core.Model;
namespace Capstone.Core.Model;

/// <summary>Anything that can supply tracks: memory for now, add a file or MusicBrainz later.</summary>
public interface ITrackSource
{
    IReadOnlyList<Track> GetAll();
}
