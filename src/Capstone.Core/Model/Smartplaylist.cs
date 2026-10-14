namespace Capstone.Core.Model;

public sealed class SmartPlaylist
{
    public SmartPlaylist(string name, SmartPlaylistRule rule)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(rule);
        if (rule.MinRating is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(rule), "MinRating must be 1 to 5, or null.");
 
        Name = name;
        Rule = rule;
    }
 
    public string Name { get; }
    public SmartPlaylistRule Rule { get; }
    public IReadOnlyList<ITrack> Evaluate(IEnumerable<ITrack> library)
    {
        ArgumentNullException.ThrowIfNull(library);
        return [.. library.Where(Rule.Matches)];
    }
}