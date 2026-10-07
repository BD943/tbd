namespace Capstone.Core;

/// <summary>Anything that can supply tracks: memory for now, add a file or MusicBrainz later.</summary>
public interface ITrackSource
{
    IReadOnlyList<Track> GetAll();
}
