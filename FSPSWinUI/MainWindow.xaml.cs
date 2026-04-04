using FSPSWinUI.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WinRT.Interop;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace FSPSWinUI;

public sealed partial class MainWindow : Window
{

    public MainWindowViewModel ViewModel { get; private set; }

    private const int SM_CYCAPTION = 4;

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadImage(IntPtr hInst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    private const uint WM_SETICON = 0x80;
    private const uint IMAGE_ICON = 1;
    private const uint LR_LOADFROMFILE = 0x00000010;
    private const int ICON_SMALL = 0;
    private const int ICON_BIG = 1;

    public MainWindow(MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();

        // Use DI-provided ViewModel instead of creating a new one
        this.ViewModel = viewModel;
        if (this.Content is FrameworkElement root)
        {
            root.DataContext = this.ViewModel;
        }

        this.Title = this.ViewModel.AppTitle;
        this.ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.AppTitle))
            {
                this.Title = this.ViewModel.AppTitle;
            }

            if (e.PropertyName == nameof(MainWindowViewModel.SelectedProfile))
            {
                var sel = this.ViewModel.SelectedProfile;
                if (sel is null)
                {
                    Debug.WriteLine("[DEBUG] SelectedProfile changed: null");
                }
                else
                {
                    Debug.WriteLine($"[DEBUG] SelectedProfile changed: Name='{sel.Name}', Path='{sel.Path}'");
                }
            }
        };

        // Extend content into the title bar so it becomes theme-aware.
        this.ExtendsContentIntoTitleBar = true;

        // Size the title bar to match system caption buttons.
        if (OperatingSystem.IsWindows())
        {
            var captionHeight = GetSystemMetrics(SM_CYCAPTION);
            if (captionHeight > 0)
            {
                TitleBarBorder.Height = Math.Max(1, captionHeight - 13);
            }
        }

        this.SetTitleBar(TitleBarBorder);

        // Keep title bar colors theme-controlled (not hard-coded).
        var appWindow = this.AppWindow;
        appWindow.TitleBar.InactiveBackgroundColor = null;
        appWindow.TitleBar.BackgroundColor = null;

        AppWindow.Resize(new Windows.Graphics.SizeInt32(1920, 1080));

        // Set taskbar icon from Assets\app.ico (unpackaged scenario).
        try
        {
            var icoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "app.ico");
            if (File.Exists(icoPath) && OperatingSystem.IsWindows())
            {
                var hwnd = WindowNative.GetWindowHandle(this);
                var hBig = LoadImage(IntPtr.Zero, icoPath, IMAGE_ICON, 0, 0, LR_LOADFROMFILE);
                var hSmall = LoadImage(IntPtr.Zero, icoPath, IMAGE_ICON, 16, 16, LR_LOADFROMFILE);
                if (hBig != IntPtr.Zero)
                {
                    SendMessage(hwnd, WM_SETICON, new IntPtr(ICON_BIG), hBig);
                }

                if (hSmall != IntPtr.Zero)
                {
                    SendMessage(hwnd, WM_SETICON, new IntPtr(ICON_SMALL), hSmall);
                }
            }
        }
        catch
        {
            // Taskbar icon is optional; ignore failures.
        }

        return;
    }


    private static void OpenExplorerAndSelect(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            var full = Path.GetFullPath(path);
            var root = Path.GetPathRoot(full) ?? string.Empty;

            // If root (e.g., "C:\") then open root directly
            if (string.Equals(full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                              root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                              StringComparison.OrdinalIgnoreCase))
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{full}\"") { UseShellExecute = true });
                return;
            }

            // If the exact path exists (folder or file), ask Explorer to select it
            if (Directory.Exists(full) || File.Exists(full))
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{full}\"") { UseShellExecute = true });
                return;
            }

            // Fallback to nearest existing parent folder
            var parent = full;
            while (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
            {
                parent = Path.GetDirectoryName(parent);
            }

            if (!string.IsNullOrEmpty(parent))
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{parent}\"") { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OpenExplorerAndSelect failed: {ex}");
        }
    }

    private async void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (!(this.Content is FrameworkElement root && root.DataContext is MainWindowViewModel vm))
        {
            return;
        }

        var selected = vm.SelectedProfile;
        if (selected is null)
        {
            return;
        }

        // Build a dialog with a checkbox that, when checked, opens Explorer at the profile's parent folder after deletion.
        var note = new TextBlock
        {
            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
        };
        note.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run { Text = "Are you sure you want to delete the profile " });
        var boldName = new Microsoft.UI.Xaml.Documents.Bold();
        boldName.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run { Text = $"\"{selected.Name}\"" });
        note.Inlines.Add(boldName);
        note.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run { Text = "?" });
        var openExplorerCheck = new Microsoft.UI.Xaml.Controls.CheckBox
        {
            Content = "Open Explorer to manually delete the folder",
            IsChecked = false,
            Margin = new Microsoft.UI.Xaml.Thickness(0, 12, 0, 0)
        };

        var panel = new Microsoft.UI.Xaml.Controls.StackPanel();
        panel.Children.Add(note);

        var infoText = new TextBlock
        {
            Text = "Important: The folder will NOT be deleted automatically. You must delete it manually.",
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
            Margin = new Microsoft.UI.Xaml.Thickness(0, 8, 0, 0)
        };
        panel.Children.Add(infoText);

        panel.Children.Add(openExplorerCheck);

        var dialog = new ContentDialog
        {
            Title = "Delete Profile",
            Content = panel,
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary,
            XamlRoot = root.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            // Remove profile from in-memory list only
            if (vm.DeleteProfileCommand.CanExecute(null))
            {
                Debug.WriteLine($"[DEBUG] Deleting profile: Name='{selected.Name}', Path='{selected.Path}'");
                vm.DeleteProfileCommand.Execute(null);
            }

            if (vm.ProfileSelectionChangedCommand.CanExecute(null))
            {
                vm.ProfileSelectionChangedCommand.Execute(null);
            }

            // If user requested, open Explorer at parent folder so they can delete manually
            if (openExplorerCheck.IsChecked == true)
            {
                try
                {
                    OpenExplorerAndSelect(selected.Path ?? string.Empty);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[WARN] Failed to open Explorer: {ex}");
                }
            }
        }
    }
}
