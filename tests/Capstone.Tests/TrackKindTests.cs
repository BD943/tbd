using Capstone.Core.Model;
 
namespace Capstone.Tests;
 
public class KindTests
{
    private static Song MakeSong(int seconds, int n = 1) =>
        new($"song-{n}", $"Song {n}", new Credit("Artist"), Genre.Rock, TimeSpan.FromSeconds(seconds), 1999, new Album($"Album {n}", "Artist", 1999));
 
    private static PodcastEpisode MakeEpisode(int seconds, int number = 12) =>
        new($"pod-{number}", $"Episode {number}", new Credit("Host"), Genre.Pop, TimeSpan.FromSeconds(seconds), 2020, new Album("The Show", "Host", 2020), number);
 
    //The switch expression: labels by length
 
    [Theory]
    [InlineData(90, "a short track")]
    [InlineData(119, "a short track")]
    [InlineData(120, "a regular track")]   // exactly 2:00 is not a short track
    [InlineData(480, "a regular track")]   // exactly 8:00 is not "over 8 minutes"
    [InlineData(481, "an epic")]
    public void A_song_is_labelled_by_its_length(int seconds, string label)
    {
        Assert.Contains(label, MakeSong(seconds).Describe());
    }
 
    [Theory]
    [InlineData(600, "a quick listen")]
    [InlineData(899, "a quick listen")]
    [InlineData(900, "a full episode")]    // exactly 15:00
    [InlineData(3600, "a full episode")]   // exactly 60:00
    [InlineData(3601, "a marathon")]
    public void A_podcast_episode_is_labelled_by_its_length(int seconds, string label)
    {
        Assert.Contains(label, MakeEpisode(seconds).Describe());
    }
 
    //Each kind describes itself differently
 
    [Fact]
    public void Each_kind_describes_itself_in_its_own_way()
    {
        Assert.StartsWith("Song:", MakeSong(200).Describe());
        Assert.StartsWith("Podcast #12:", MakeEpisode(200).Describe());
    }
 
    [Fact]
    public void A_long_episode_shows_its_minutes_past_59()
    {
        Assert.Contains("75:00", MakeEpisode(75 * 60).Describe());
    }
 
    //The polymorphic loop 
 
    [Fact]
    public void DescribeAll_lets_each_track_describe_itself_in_order()
    {
        var tracks = new List<ITrack> { MakeSong(200, 1), MakeEpisode(1800, 7), MakeSong(600, 2) };
 
        var lines = LibraryReport.DescribeAll(tracks);
 
        Assert.Equal(3, lines.Count);
        Assert.StartsWith("Song:", lines[0]);
        Assert.StartsWith("Podcast #7:", lines[1]);
        Assert.StartsWith("Song:", lines[2]);
    }
 
    //Track Kinds share the base behaviour 
 
    [Fact]
    public void Both_kinds_join_the_album_and_count_toward_its_length()
    {
        var album = new Album("Mixed", "Artist", 1999);
        new Song("song-1", "S", new Credit("A"), Genre.Rock, new TimeSpan(0, 3, 0), 1999, album);
        new PodcastEpisode("pod-1", "P", new Credit("H"), Genre.Pop, new TimeSpan(0, 30, 0), 1999, album, 1);
 
        Assert.Equal(2, album.Tracks.Count);
        Assert.Equal(new TimeSpan(0, 33, 0), album.TotalDuration);
    }
 
    [Fact]
    public void A_podcast_episode_keeps_its_kind_and_number_through_json()
    {
        var mapper = new TrackMapper();
 
        var copy = mapper.FromJson(mapper.ToJson(MakeEpisode(1800, 7)));
 
        var episode = Assert.IsType<PodcastEpisode>(copy);
        Assert.Equal(7, episode.EpisodeNumber);
    }
}