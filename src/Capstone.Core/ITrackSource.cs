namespace Capstone.Core;

/// <summary>Anything that can supply tracks: memory now, a file or MusicBrainz later.</summary>
public interface ITrackSource
{
    IReadOnlyList<Track> GetAll();
}
