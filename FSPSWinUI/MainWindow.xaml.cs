using FSPSWinUI.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using WinRT.Interop;

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

    private async void AddProfileButton_Click(object sender, RoutedEventArgs e)
    {


        if (!(this.Content is FrameworkElement root && root.DataContext is MainWindowViewModel vm))
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "New Profile",
            PrimaryButtonText = "OK",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        // Ensure the dialog is associated with this window's XamlRoot so it appears correctly
        dialog.XamlRoot = root.XamlRoot;

        var textBox = new TextBox
        {
            PlaceholderText = "enter profile name",
            Margin = new Thickness(0, 8, 0, 0)
        };

        // Validation: enable OK only when non-empty and not duplicate (case-insensitive, trimmed)
        void UpdateOkEnabled()
        {
            var trimmed = (textBox.Text ?? string.Empty).Trim();
            var exists = vm.Profiles.Any(p => string.Equals((p.Name ?? string.Empty).Trim(), trimmed, StringComparison.OrdinalIgnoreCase));
            dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(trimmed) && !exists;
        }

        textBox.TextChanged += (s, args) => UpdateOkEnabled();

        dialog.Content = textBox;
        dialog.IsPrimaryButtonEnabled = false;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var name = (textBox.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            var exists = vm.Profiles.Any(p => string.Equals((p.Name ?? string.Empty).Trim(), name, StringComparison.OrdinalIgnoreCase));
            if (exists)
            {
                return;
            }

            var profile = new FSPSLibrary.Models.Profile { Name = name };
            vm.AddProfile(profile);
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

        var dialog = new ContentDialog
        {
            Title = "Delete Profile",
            Content = $"Are you sure you want to delete the profile '{selected.Name}'?",
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary
        };
        dialog.XamlRoot = root.XamlRoot;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            if (vm.DeleteProfileCommand.CanExecute(null))
            {
                vm.DeleteProfileCommand.Execute(null);
            }

            if (vm.ProfileSelectionChangedCommand.CanExecute(null))
            {
                vm.ProfileSelectionChangedCommand.Execute(null);
            }
        }
    }
}
