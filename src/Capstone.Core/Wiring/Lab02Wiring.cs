using Capstone.Core.Model;
using SharpQuest.Contracts;
namespace Capstone.Core.Wiring;

/// <summary>Tells the Lab 02 tests how to create this capstone's objects.</summary>
public sealed class Lab02Wiring : ILab02Wiring
{
    public IItemStore CreateStore() => new ItemStore();

    public IItemMapper CreateMapper() => new TrackMapper();

    public IItem CreateSample(int seed)
    {
        Genre[] genres = Enum.GetValues<Genre>();
        int year = 1970 + Math.Abs(seed % 50);

        var album = new Album($"Sample Album {seed}", "Sample Artist", year);

        return new Track(
            id: $"trk-{seed:D3}",
            title: $"Sample Track {seed}",
            credit: new Credit("Sample Artist"),
            genre: genres[Math.Abs(seed % genres.Length)],
            duration: TimeSpan.FromSeconds(180 + Math.Abs(seed % 100)),
            year: year,
            album: album);
    }
}
