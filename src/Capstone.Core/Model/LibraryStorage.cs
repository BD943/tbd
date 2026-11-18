using System.Text.Json;
using SharpQuest.Contracts;

namespace Capstone.Core.Model;

//Saves a library to a JSON file and reads it back, each item as its own kind.
public static class LibraryStorage
{
    private const int CurrentVersion = 1;

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    /// <summary>
    /// Writes everything the store holds to <paramref name="path"/>, replacing what was there.
    /// A missing folder is created. The file is a snapshot of the store at this moment.
    /// </summary>
    public static async Task SaveAsync(IItemStore store, string path)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        // Each item goes through TrackMapper on its own, because the mapper knows which kind it is.
        var mapper = new TrackMapper();
        var items = new List<JsonElement>();
        foreach (var item in store.All())
        {
            if (item is not Track)
                throw new NotSupportedException($"Only tracks can be saved, not {item.GetType().Name}.");

            items.Add(JsonSerializer.Deserialize<JsonElement>(mapper.ToJson(item)));
        }

        string text = JsonSerializer.Serialize(new LibraryFileDto(CurrentVersion, items), Options);

        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        /* Write beside the target, then swap it in: a crash half way through writing
         leaves the previous file whole instead of half a new one.*/
        string temp = path + ".tmp";
        try
        {
            await File.WriteAllTextAsync(temp, text);
            File.Move(temp, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temp))
                File.Delete(temp);
        }
    }

    /// <summary>
    /// The library as it was saved. A file that isn't there is an empty library (nothing saved yet).
    /// A file that is there but isn't a library file throws InvalidDataException.
    /// </summary>
    public static async Task<Library> LoadAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var library = new Library();
        if (!File.Exists(path))
            return library;

        string text = await File.ReadAllTextAsync(path);

        LibraryFileDto? file;
        try
        {
            file = JsonSerializer.Deserialize<LibraryFileDto>(text, Options);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"'{path}' is not a library file.", ex);
        }

        if (file is null || file.Version != CurrentVersion || file.Items is null)
            throw new InvalidDataException($"'{path}' is not a library file of version {CurrentVersion}.");

        var mapper = new TrackMapper();
        foreach (var element in file.Items)
        {
            var item = mapper.FromJson(element.GetRawText()) ?? throw new InvalidDataException($"'{path}' holds an entry that is not a valid track.");

            try
            {
                library.Add(item);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidDataException($"'{path}' holds two items with the same Id.", ex);
            }
        }

        return library;
    }
}