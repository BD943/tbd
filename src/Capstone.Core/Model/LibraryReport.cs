namespace Capstone.Core.Model;
 
public static class LibraryReport
{
    /// <summary>
    /// One description line per track, in order. The loop only knows ITrack;
    /// each track answers Describe() in the way its own kind does (polymorphism).
    /// </summary>
    public static IReadOnlyList<string> DescribeAll(IEnumerable<ITrack> tracks)
    {
        ArgumentNullException.ThrowIfNull(tracks);
 
        var lines = new List<string>();
        foreach (ITrack track in tracks)
        {
            lines.Add(track.Describe());
        }
        return lines;
    }
}