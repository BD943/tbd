using System.Collections.Specialized;
using Capstone.Core.Model;
using Capstone.Core.ViewModels;

namespace Capstone.Tests;

public class CatalogueViewModelTests
{
    private static Song MakeSong(int n, double minutes = 3, string? title = null) =>
        new($"song-{n:D3}", title ?? $"Song {n}", new Credit("Artist"), Genre.Rock, TimeSpan.FromMinutes(minutes), 2000, new Album("Album", "Artist", 2000));

    private static Library LibraryOf(params string[] titles)
    {
        var library = new Library();
        for (int n = 0; n < titles.Length; n++)
            library.Add(MakeSong(n + 1, title: titles[n]));
        return library;
    }

    //Search 

    [Fact]
    public void The_search_narrows_the_list_and_leaves_the_library_alone()
    {
        var library = LibraryOf("Alpha", "Beta");
        var vm = new CatalogueViewModel(library);

        vm.Filter = "alpha";

        Assert.Equal(new[] { "song-001" }, vm.Items.Select(i => i.Id));
        Assert.Equal(2, library.Count);
    }

    [Fact]
    public void Clearing_the_search_brings_every_row_back()
    {
        var vm = new CatalogueViewModel(LibraryOf("Alpha", "Beta", "Gamma"));
        vm.Filter = "beta";

        vm.Filter = "";

        Assert.Equal(3, vm.Items.Count());
    }

    //The list follows the library 

    [Fact]
    public void Adding_to_the_library_adds_exactly_one_row()
    {
        var library = LibraryOf("Alpha");
        var vm = new CatalogueViewModel(library);
        var actions = new List<NotifyCollectionChangedAction>();
        ((INotifyCollectionChanged)vm.Items).CollectionChanged += (_, e) => actions.Add(e.Action);

        library.Add(MakeSong(2));

        Assert.Equal(new[] { NotifyCollectionChangedAction.Add }, actions);
        Assert.Equal(2, vm.Items.Count());
    }

    [Fact]
    public void New_track_that_does_not_match_the_search_is_not_shown()
    {
        var library = LibraryOf("Alpha");
        var vm = new CatalogueViewModel(library);
        vm.Filter = "alpha";

        library.Add(MakeSong(2, title: "Beta"));

        Assert.Single(vm.Items);
        Assert.Equal(2, library.Count);
    }

    [Fact]
    public void Deleting_the_selected_track_removes_row_and_clears_selection()
    {
        var library = LibraryOf("Alpha", "Beta");
        var vm = new CatalogueViewModel(library);
        vm.Selected = vm.Items.First();

        vm.RemoveCommand.Execute(null);

        Assert.Null(vm.Selected);
        Assert.Single(vm.Items);
        Assert.Equal(1, library.Count);
    }

    //The buttons

    [Fact]
    public void Delete_is_greyed_out_until_something_is_selected_and_the_button_is_told()
    {
        var vm = new CatalogueViewModel(LibraryOf("Alpha"));
        int asked = 0;
        vm.RemoveCommand.CanExecuteChanged += (_, _) => asked++;
        Assert.False(vm.RemoveCommand.CanExecute(null));

        vm.Selected = vm.Items.First();

        Assert.True(vm.RemoveCommand.CanExecute(null));
        Assert.True(asked > 0, "The selection changed but the button was never told to ask again.");
    }

    //The numbers and the details panel 

    [Fact]
    public void The_count_and_the_total_length_follow_adds_and_deletes()
    {
        var library = new Library();
        var vm = new CatalogueViewModel(library);

        library.Add(MakeSong(1, minutes: 3));
        library.Add(MakeSong(2, minutes: 3));
        Assert.Equal(2, vm.Count);
        Assert.Equal("0:06:00", vm.TotalLengthText);

        library.Remove("song-001");
        Assert.Equal(1, vm.Count);
        Assert.Equal("0:03:00", vm.TotalLengthText);
    }

    [Fact]
    public void The_details_say_so_when_nothing_is_selected_and_describe_the_selection_otherwise()
    {
        var vm = new CatalogueViewModel(LibraryOf("Alpha"));
        Assert.Equal("Nothing selected.", vm.Details);

        vm.Selected = vm.Items.First();

        Assert.Contains("Alpha", vm.Details);
    }
}
