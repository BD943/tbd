using Capstone.Core.Model;

namespace Capstone.Tests;

public class PersistenceTests : IDisposable
{
    private readonly string folder =
        Path.Combine(Path.GetTempPath(), "capstone-tests", Guid.NewGuid().ToString("N"));

    private string PathFor(string fileName = "library.json") => Path.Combine(folder, fileName);

    private static Song MakeSong(string id, int plays = 0, int? rating = null, double seconds = 200) =>
        new(id, $"Song {id}", new Credit("Artist"), Genre.Rock, TimeSpan.FromSeconds(seconds), 1999,
        new Album($"Album {id}", "Artist", 1999), playCount: plays, rating: rating);

    public void Dispose()
    {
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
        GC.SuppressFinalize(this);
    }
    [Fact]
    public async Task Play_counts_ratings_survive_the_file()
    {
        var library = new Library();
        library.Add(MakeSong("a", plays: 7, rating: 4));
        library.Add(MakeSong("b"));
        var path = PathFor();

        await LibraryStorage.SaveAsync(library, path);
        var loaded = await LibraryStorage.LoadAsync(path);

        var a = Assert.IsType<Song>(loaded["a"]);
        Assert.Equal(7, a.PlayCount);
        Assert.Equal(4, a.Rating);

        var b = Assert.IsType<Song>(loaded["b"]);
        Assert.Equal(0, b.PlayCount);
        Assert.Null(b.Rating);
    }

    [Fact]
    public async Task A_duration_comes_back_exactly()
    {
        var duration = TimeSpan.FromMilliseconds(215_500);
        var library = new Library();
        library.Add(MakeSong("a", seconds: duration.TotalSeconds));
        var path = PathFor();

        await LibraryStorage.SaveAsync(library, path);
        var loaded = await LibraryStorage.LoadAsync(path);

        Assert.Equal(duration, Assert.IsType<Song>(loaded["a"]).Duration);
    }

    [Fact]
    public async Task Each_kind_comes_back_its_own_kind()
    {
        var library = new Library();
        library.Add(MakeSong("song"));
        library.Add(new PodcastEpisode("pod", "Episode", new Credit("Host"), Genre.Pop, new TimeSpan(0, 45, 0), 2020, new Album("The Show", "Host", 2020), episodeNumber: 12));
        var path = PathFor();

        await LibraryStorage.SaveAsync(library, path);
        var loaded = await LibraryStorage.LoadAsync(path);

        Assert.IsType<Song>(loaded["song"]);
        var episode = Assert.IsType<PodcastEpisode>(loaded["pod"]);
        Assert.Equal(12, episode.EpisodeNumber);
    }

    [Fact]
    public async Task The_order_is_kept()
    {
        var library = new Library();
        library.Add(MakeSong("c"));
        library.Add(MakeSong("a"));
        library.Add(MakeSong("b"));
        var path = PathFor();

        await LibraryStorage.SaveAsync(library, path);
        var loaded = await LibraryStorage.LoadAsync(path);

        Assert.Equal(new[] { "c", "a", "b" }, loaded.Select(item => item.Id));
    }

    [Fact]
    public async Task Saving_again_replaces_file_and_leaves_no_temporary_file()
    {
        var path = PathFor();
        var first = new Library();
        first.Add(MakeSong("a"));
        first.Add(MakeSong("b"));
        first.Add(MakeSong("c"));
        var second = new Library();
        second.Add(MakeSong("x"));

        await LibraryStorage.SaveAsync(first, path);
        await LibraryStorage.SaveAsync(second, path);
        var loaded = await LibraryStorage.LoadAsync(path);

        Assert.Equal(1, loaded.Count);
        Assert.False(File.Exists(path + ".tmp"), "A temporary file was left behind.");
    }

    [Fact]
    public async Task A_file_that_is_not_there_is_an_empty_library()
    {
        var path = PathFor("nothing-saved-yet.json");

        var loaded = await LibraryStorage.LoadAsync(path);

        Assert.Equal(0, loaded.Count);
        Assert.False(File.Exists(path), "Loading must not create the file.");
    }

    [Fact]
    public async Task Saving_creates_the_folder_it_needs()
    {
        var path = PathFor(Path.Combine("not-created-yet", "library.json"));

        await LibraryStorage.SaveAsync(new Library(), path);

        Assert.True(File.Exists(path));
    }

    [Fact]
    public async Task Nonsense_in_the_file_is_error()
    {
        Directory.CreateDirectory(folder);
        var path = PathFor();
        await File.WriteAllTextAsync(path, "{ this is not json at all ");

        await Assert.ThrowsAsync<InvalidDataException>(() => LibraryStorage.LoadAsync(path));
    }

    [Fact]
    public async Task Json_of_the_wrong_shape_is_error()
    {
        Directory.CreateDirectory(folder);
        var path = PathFor();
        await File.WriteAllTextAsync(path, """{ "hello": "world" }""");

        await Assert.ThrowsAsync<InvalidDataException>(() => LibraryStorage.LoadAsync(path));
    }

    [Fact]
    public async Task Half_a_file_is_error()
    {
        var library = new Library();
        library.Add(MakeSong("a"));
        library.Add(MakeSong("b"));
        var path = PathFor();
        await LibraryStorage.SaveAsync(library, path);
        var whole = await File.ReadAllTextAsync(path);
        await File.WriteAllTextAsync(path, whole[..(whole.Length / 2)]);

        await Assert.ThrowsAsync<InvalidDataException>(() => LibraryStorage.LoadAsync(path));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task A_blank_path_is_programming_error(string path)
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(() => LibraryStorage.SaveAsync(new Library(), path));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => LibraryStorage.LoadAsync(path));
    }

    [Fact]
    public async Task A_null_store_or_path_is_programming_error()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => LibraryStorage.SaveAsync(null!, PathFor()));
        await Assert.ThrowsAsync<ArgumentNullException>(() => LibraryStorage.SaveAsync(new Library(), null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => LibraryStorage.LoadAsync(null!));
    }
}