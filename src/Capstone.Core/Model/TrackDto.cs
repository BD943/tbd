using Capstone.Core.Model;
namespace Capstone.Core.Model;

/// <summary>
/// The JSON shape of a <see cref="Track"/>. Every field that can be absent is nullable,
/// so a missing required field can be told apart from a default value.
/// </summary>
internal sealed record TrackDto(
    string? Id,
    string? Title,
    string? Artist,
    string? FeaturedArtist,
    Genre? Genre,
    double? DurationSeconds,
    int? Year,
    string? AlbumTitle,
    string? AlbumArtist,
    int? AlbumYear,
    int PlayCount,
    int? Rating,
    string? Kind,
    int? EpisodeNumber);