using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSPSWinUI;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly ViewModels.MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new ViewModels.MainWindowViewModel();

        // Set DataContext on the root content element (Window doesn't expose DataContext).
        if (this.Content is FrameworkElement root)
        {
            root.DataContext = _viewModel;
        }

        // Load title from appsettings.json
        try
        {
            var json = System.IO.File.ReadAllText(System.IO.Path.Combine(AppContext.BaseDirectory, "appsettings.json"));
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("App", out var appSection) && appSection.TryGetProperty("Title", out var titleElement))
            {
                _viewModel.AppTitle = titleElement.GetString() ?? _viewModel.AppTitle;
            }
        }
        catch
        {
            // ignore - fallback to default
        }

        // Ensure window title reflects ViewModel and updates when it changes.
        this.Title = _viewModel.AppTitle;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModels.MainWindowViewModel.AppTitle))
            {
                this.Title = _viewModel.AppTitle;
            }
        };

        // Try to set an initial window size (Width = 500, Height = 700) using AppWindow when available.
        try
        {
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            if (appWindow is not null)
            {
                appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 1920, Height = 1080 });
            }
        }
        catch
        {
            // ignore - running in environments where AppWindow API isn't available.
        }
    }
}
