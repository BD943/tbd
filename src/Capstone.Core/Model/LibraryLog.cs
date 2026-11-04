using SharpQuest.Contracts;
namespace Capstone.Core.Model;

public sealed class LibraryLog : IDisposable
{
    private readonly Library library;
    private readonly List<string> lines = [];
 
    public LibraryLog(Library library)
    {
        ArgumentNullException.ThrowIfNull(library);
        this.library = library;
        library.ItemAdded += OnItemAdded;   // subscribe
    }
 
    public IReadOnlyList<string> Lines => lines;
 
    private void OnItemAdded(IItem item) => lines.Add($"Added: {item.DisplayName}");
 
    public void Dispose() => library.ItemAdded -= OnItemAdded;   // unsubscribe
}