using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Input;
using Capstone.Core.Model;
using SharpQuest.Contracts;

namespace Capstone.Core.ViewModels;

/// <summary>
/// Everything the window shows: the list, the search box,
/// the selected row, the details line, the counts and the two buttons.
/// It listens to the library's events, so the list follows the library without anyone telling it to.
/// </summary>
public sealed class CatalogueViewModel : ICatalogueViewModel, IDisposable
{
    private readonly Library library;
    private readonly SearchMatcher matcher = SearchMatcher.Create();   // the Lab 03 search rules
    private readonly ObservableCollection<IItem> items = new();        // the one list the window binds to
    private readonly RelayCommand removeCommand;
    private readonly RelayCommand addTrackCommand;

    private string filter = "";
    private IItem? selected;
    private int lastCount;
    private string lastTotal;

    // The "Add a track" form.
    private string newTitle = "";
    private string newArtist = "";
    private string newMinutes = "3";
    private Genre newGenre = Genre.Pop;

    public CatalogueViewModel(Library library)
    {
        ArgumentNullException.ThrowIfNull(library);
        this.library = library;

        foreach (var item in library)
            items.Add(item);

        AddCommand = new RelayCommand(Add);
        removeCommand = new RelayCommand(_ => Remove(), _ => Selected is not null);
        addTrackCommand = new RelayCommand(_ => AddTrackFromForm(), _ => TryReadForm(out string _, out string _, out double _));

        lastCount = items.Count;
        lastTotal = TotalLengthText;

        library.ItemAdded += OnItemAdded;
        library.ItemRemoved += OnItemRemoved;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    //What the window binds to

    //The same collection object for the whole life of the view model. Changes are made in place.
    public IEnumerable<IItem> Items => items;

    public string Filter
    {
        get => filter;
        set
        {
            var next = value ?? "";   // a binding hands over null when the box is cleared
            if (next == filter)
                return;               // same value: announce nothing, redraw nothing

            filter = next;
            Raise(nameof(Filter));
            Refresh();
        }
    }

    public IItem? Selected
    {
        get => selected;
        set
        {
            if (ReferenceEquals(selected, value))
                return;

            selected = value;
            Raise(nameof(Selected));
            Raise(nameof(Details));                // Details is computed from Selected, so it moved too
            removeCommand.RaiseCanExecuteChanged(); // and so did the answer to "can I delete?"
        }
    }

    public string Details => selected switch
    {
        null => "Nothing selected.",
        ITrack track => $"{track.Describe()} · {Plays(track.PlayCount)} · {Rated(track.Rating)} · Id {track.Id}",
        var item => $"{item.DisplayName} · Id {item.Id}"
    };

    //How many rows are showing.
    public int Count => items.Count;

    //The total length of the tracks showing, as hours:minutes:seconds.
    public string TotalLengthText
    {
        get
        {
            var total = TimeSpan.FromTicks(items.OfType<ITrack>().Sum(track => track.Duration.Ticks));
            return $"{(int)total.TotalHours}:{total.Minutes:D2}:{total.Seconds:D2}";
        }
    }

    public ICommand AddCommand { get; }

    public ICommand RemoveCommand => removeCommand;

    //The "Add a track" form 

    public string NewTitle
    {
        get => newTitle;
        set => SetFormField(ref newTitle, value ?? "", nameof(NewTitle));
    }

    public string NewArtist
    {
        get => newArtist;
        set => SetFormField(ref newArtist, value ?? "", nameof(NewArtist));
    }

    //The length as typed, in minutes.
    public string NewMinutes
    {
        get => newMinutes;
        set => SetFormField(ref newMinutes, value ?? "", nameof(NewMinutes));
    }

    public Genre NewGenre
    {
        get => newGenre;
        set
        {
            if (newGenre == value)
                return;
            newGenre = value;
            Raise(nameof(NewGenre));
        }
    }

    //What the genre ComboBox lists.
    public IReadOnlyList<Genre> Genres { get; } = Enum.GetValues<Genre>();

    //The form's Add button: greyed out until there is a title, an artist and a length above 0.
    public ICommand AddTrackCommand => addTrackCommand;

    private void Add(object? parameter)
    {
        if (parameter is not IItem item)
            throw new ArgumentException("AddCommand needs the item to add as its parameter.", nameof(parameter));

        library.Add(item);   // through the store: its rules apply, and its event updates the list
    }

    private void AddTrackFromForm()
    {
        if (!TryReadForm(out var title, out var artist, out var minutes))
            return;

        var year = DateTime.Now.Year;
        var song = new Song("song-" + Guid.NewGuid().ToString("N")[..8], title, new Credit(artist), newGenre, TimeSpan.FromMinutes(minutes), year, new Album("Singles", artist, year));

        AddCommand.Execute(song);   // the same path the tests use
        NewTitle = "";
    }

    private bool TryReadForm(out string title, out string artist, out double minutes)
    {
        title = newTitle.Trim();
        artist = newArtist.Trim();
        var parsed = double.TryParse(newMinutes, NumberStyles.Float, CultureInfo.CurrentCulture, out minutes) || double.TryParse(newMinutes, NumberStyles.Float, CultureInfo.InvariantCulture, out minutes);
        return title.Length > 0 && artist.Length > 0 && parsed && minutes > 0;
    }

    private void SetFormField(ref string field, string value, string name)
    {
        if (field == value)
            return;
        field = value;
        Raise(name);
        addTrackCommand.RaiseCanExecuteChanged(); 
    }

    private void Remove()
    {
        if (selected is null)
            return;
        library.Remove(selected.Id);   // the library's event takes it out of the list
    }

    //Keeping the list in step with the library

    private void OnItemAdded(IItem item)
    {
        if (!Matches(item))
            return;          // the filter decides what is shown, new arrivals included

        items.Add(item);     // a new item is last in the library, so it is last here: exactly one Add
        UpdateTotals();
    }

    private void OnItemRemoved(IItem item)
    {
        if (ReferenceEquals(selected, item))
            Selected = null; // never point at something that is gone

        items.Remove(item);
        UpdateTotals();
    }

    /// <summary>
    /// Makes the shown list match the filter by removing and inserting rows in place,
    /// not by clearing and refilling (a Clear would tell the window to redraw everything).
    /// </summary>
    private void Refresh()
    {
        var wanted = library.Where(Matches).ToList();   // library order
        var wantedSet = new HashSet<IItem>(wanted);

        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (!wantedSet.Contains(items[i]))
                items.RemoveAt(i);
        }

        // What is left is in library order, so anything out of place is simply missing.
        for (int i = 0; i < wanted.Count; i++)
        {
            if (i >= items.Count || !ReferenceEquals(items[i], wanted[i]))
                items.Insert(i, wanted[i]);
        }

        if (selected is not null && !items.Contains(selected))
            Selected = null; // a hidden row can't stay selected

        UpdateTotals();
    }

    private bool Matches(IItem item) => matcher.Matches(item.DisplayName, filter);

    private void UpdateTotals()
    {
        if (items.Count != lastCount)
        {
            lastCount = items.Count;
            Raise(nameof(Count));
        }

        var total = TotalLengthText;
        if (total != lastTotal)
        {
            lastTotal = total;
            Raise(nameof(TotalLengthText));
        }
    }

    private void Raise(string property) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    private static string Plays(int count) => $"{count} play{(count == 1 ? "" : "s")}";

    private static string Rated(int? rating) => rating is { } stars ? $"rated {stars}/5" : "not rated yet";

    //Stops listening to the library. Call it when the window closes.
    public void Dispose()
    {
        library.ItemAdded -= OnItemAdded;
        library.ItemRemoved -= OnItemRemoved;
    }
}
