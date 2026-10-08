using SharpQuest.Contracts;

namespace Capstone.Core.Model;

public interface ITrack : IItem
{
    string Title { get; }
    Credit Credit { get; }
    Genre Genre { get; }
    TimeSpan Duration { get; }
    int Year { get; }
    Album Album { get; }
    int PlayCount { get; }
    int? Rating { get; }
}