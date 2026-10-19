using System.Text.Json;
using System.Text.Json.Serialization;
using SharpQuest.Contracts;
 
namespace Capstone.Core.Model;
 
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
 
        string kind = track switch
        {
            PodcastEpisode => "podcast",
            Song => "song",
            _ => throw new ArgumentException($"Unknown kind of track: {track.GetType().Name}.", nameof(item))
        };
        int? episodeNumber = track is PodcastEpisode episode ? episode.EpisodeNumber : null;
 
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
            track.Rating,
            kind,
            episodeNumber);
 
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
            var credit = new Credit(dto.Artist, dto.FeaturedArtist);
            var duration = TimeSpan.FromSeconds(dto.DurationSeconds.Value);
 
            // A missing kind is read as a song, so JSON written before kinds existed still loads.
            Track? track = dto.Kind switch
            {
                null or "song" =>
                    new Song(dto.Id, dto.Title, credit, dto.Genre.Value, duration, dto.Year.Value, album, dto.PlayCount, dto.Rating),
                "podcast" when dto.EpisodeNumber is { } number =>
                    new PodcastEpisode(dto.Id, dto.Title, credit, dto.Genre.Value, duration, dto.Year.Value, album, number, dto.PlayCount, dto.Rating),
                _ => null   // unknown kind, or a podcast without an episode number
            };
 
            return track;
        }
        catch (Exception e) when (e is JsonException or ArgumentException or OverflowException)
        {
            // Bad text, or values the model rejects (rating 9, negative plays, ...): never throw.
            return null;
        }
    }
}