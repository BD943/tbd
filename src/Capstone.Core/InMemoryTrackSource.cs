namespace Capstone.Core;

public sealed class InMemoryTrackSource : ITrackSource
{
    private readonly List<Track> tracks;

    public InMemoryTrackSource(IEnumerable<Track> tracks)
    {
        ArgumentNullException.ThrowIfNull(tracks);
        this.tracks = [.. tracks];
    }

    public IReadOnlyList<Track> GetAll() => tracks;
}
