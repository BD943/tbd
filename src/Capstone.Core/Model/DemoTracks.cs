namespace Capstone.Core.Model;

public static class DemoTracks
{
    public static void AddTo(Library library)
    {
        ArgumentNullException.ThrowIfNull(library);

        var abbeyRoad = new Album("Abbey Road", "The Beatles", 1969);
        var nevermind = new Album("Nevermind", "Nirvana", 1991);

        library.Add(new Song("song-001", "Come Together", new Credit("The Beatles"), Genre.Rock, new TimeSpan(0, 4, 20), 1969, abbeyRoad, playCount: 12, rating: 5));
        library.Add(new Song("song-002", "Something", new Credit("The Beatles"), Genre.Rock, new TimeSpan(0, 3, 3), 1969, abbeyRoad));
        library.Add(new Song("song-003", "Smells Like Teen Spirit", new Credit("Nirvana"), Genre.Rock, new TimeSpan(0, 5, 1), 1991, nevermind, playCount: 40, rating: 4));
        library.Add(new Song("song-004", "Lithium", new Credit("Nirvana"), Genre.Rock, new TimeSpan(0, 4, 17), 1991, nevermind));
    }
}