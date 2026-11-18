using System.Text.Json;

namespace Capstone.Core.Model;

/// <summary>
/// The shape of the library file. Items holds one JSON object per track, exactly as TrackMapper writes it,
/// so each entry says which kind it is. Version lets a future format be told apart from this one.
/// </summary>
internal sealed record LibraryFileDto(int Version, List<JsonElement>? Items);