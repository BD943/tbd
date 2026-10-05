using System.Text.Json;
using System.Text.Json.Serialization;
using SharpQuest.Contracts;

namespace Capstone.Core;

public sealed class TrackMapper : IItemMapper
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public string ToJson(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item is not Track track)
            throw new ArgumentException("Only Track items can be serialised.", nameof(item));

        var dto = new TrackDto(
            track.Id,
            track.Title,
            track.Credit.Artist,
            track.Credit.FeaturedArtist,
            track.Genre,
            track.Duration.TotalSeconds,
            track.Year,
            track.Album.Title,
            track.Album.Artist,
            track.Album.ReleaseYear,
            track.PlayCount,
            track.Rating);

        return JsonSerializer.Serialize(dto, Options);
    }

    public IItem? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var dto = JsonSerializer.Deserialize<TrackDto>(json, Options);

            if (dto is null
                || string.IsNullOrWhiteSpace(dto.Id)
                || string.IsNullOrWhiteSpace(dto.Title)
                || string.IsNullOrWhiteSpace(dto.Artist)
                || dto.Genre is null
                || dto.DurationSeconds is null
                || dto.Year is null
                || string.IsNullOrWhiteSpace(dto.AlbumTitle)
                || string.IsNullOrWhiteSpace(dto.AlbumArtist)
                || dto.AlbumYear is null)
            {
                return null;
            }

            var album = new Album(dto.AlbumTitle, dto.AlbumArtist, dto.AlbumYear.Value);

            return new Track(
                dto.Id,
                dto.Title,
                new Credit(dto.Artist, dto.FeaturedArtist),
                dto.Genre.Value,
                TimeSpan.FromSeconds(dto.DurationSeconds.Value),
                dto.Year.Value,
                album,
                dto.PlayCount,
                dto.Rating);
        }
        catch (Exception e) when (e is JsonException or ArgumentException or OverflowException)
        {
            // Bad text, or values our model rejects (rating 9, negative plays, ...): never throw.
            return null;
        }
    }
}