using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Capstone.Core.Model;
using Capstone.Core.ViewModels;
using Capstone.Core.Wiring;

namespace Capstone.Ui;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // The window gets its view model here, once.
            var wiring = new Lab09Wiring();
            var store = (Library)wiring.CreateStore();
            DemoTracks.AddTo(store);   // or: for (var i = 1; i <= 5; i++) store.Add(wiring.CreateSample(i));

            var viewModel = (CatalogueViewModel)wiring.CreateViewModel(store);
            desktop.MainWindow = new MainWindow { DataContext = viewModel };
            desktop.MainWindow.Closed += (_, _) => viewModel.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}