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

    private async void AddProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (!(this.Content is FrameworkElement root && root.DataContext is MainWindowViewModel vm))
        {
            return;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = root.XamlRoot,
            Title = "New Profile",
            PrimaryButtonText = "OK",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            HorizontalContentAlignment = HorizontalAlignment.Stretch
        };

        // Build dialog content using a single Grid (2 columns x 4 rows) inside the fixed-width Border wrapper.
        var rootGrid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(0)
        };
        // Columns: TextBox (star) | Button (auto)
        rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        // Rows 0..3: name label, name box, path label, path row
        rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var nameLabel = new TextBlock
        {
            Text = "Name",
            Margin = new Thickness(0, 0, 0, 6),
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        Grid.SetRow(nameLabel, 0);
        Grid.SetColumn(nameLabel, 0);
        Grid.SetColumnSpan(nameLabel, 2);
        rootGrid.Children.Add(nameLabel);

        var nameTextBox = new TextBox
        {
            PlaceholderText = "enter profile name",
            Margin = new Thickness(0, 0, 0, 8),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinHeight = 32
        };
        Grid.SetRow(nameTextBox, 1);
        Grid.SetColumn(nameTextBox, 0);
        Grid.SetColumnSpan(nameTextBox, 2);
        rootGrid.Children.Add(nameTextBox);

        var pathLabel = new TextBlock
        {
            Text = "Folder path",
            Margin = new Thickness(0, 0, 0, 6),
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        Grid.SetRow(pathLabel, 2);
        Grid.SetColumn(pathLabel, 0);
        Grid.SetColumnSpan(pathLabel, 2);
        rootGrid.Children.Add(pathLabel);

        // Row 3: Path TextBox in col 0 (1*) and Browse Button in col 1 (Auto)
        var pathTextBox = new TextBox
        {
            PlaceholderText = "enter or pick a folder path",
            Margin = new Thickness(0, 0, 10, 0),
            MinHeight = 32,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        Grid.SetRow(pathTextBox, 3);
        Grid.SetColumn(pathTextBox, 0);
        rootGrid.Children.Add(pathTextBox);

        var browseButton = new Button
        {
            Content = "\u2026",
            Width = 48,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(browseButton, 3);
        Grid.SetColumn(browseButton, 1);
        rootGrid.Children.Add(browseButton);

        // Set minimum width on pathTextBox to ensure dialog is wide enough for comfortable input
        pathTextBox.MinWidth = 400; // Adjust as needed for usability

        dialog.Content = rootGrid; // No Border wrapper needed
        dialog.IsPrimaryButtonEnabled = false;

        // Validation: name non-empty & unique, path absolute & not duplicate
        void UpdateOkEnabled()
        {
            var name = (nameTextBox.Text ?? string.Empty).Trim();
            var path = (pathTextBox.Text ?? string.Empty).Trim();

            var nameExists = vm.Profiles.Any(p => string.Equals((p.Name ?? string.Empty).Trim(), name, StringComparison.OrdinalIgnoreCase));
            var pathExists = vm.Profiles.Any(p => string.Equals((p.Path ?? string.Empty).Trim(), path, StringComparison.OrdinalIgnoreCase));

            var isNameValid = !string.IsNullOrWhiteSpace(name) && !nameExists;

            var isPathValid = false;
            if (!string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    // Require absolute paths only
                    if (Path.IsPathRooted(path) && Path.IsPathFullyQualified(path))
                    {
                        // Try get full path to validate characters/format
                        _ = Path.GetFullPath(path);
                        isPathValid = !pathExists;
                    }
                }
                catch
                {
                    isPathValid = false;
                }
            }

            dialog.IsPrimaryButtonEnabled = isNameValid && isPathValid;
        }

        nameTextBox.TextChanged += (s, args) => UpdateOkEnabled();
        pathTextBox.TextChanged += (s, args) => UpdateOkEnabled();

        // Browse button opens WinRT FolderPicker (WinUI-friendly). Note: FolderPicker cannot reliably open at an arbitrary
        // start path on all platforms; the user chose the WinRT picker.
        browseButton.Click += async (s, args) =>
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FolderPicker();
                // Associate picker with this window
                WinRT.Interop.InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));
                picker.FileTypeFilter.Add("*");

                // If user has entered an existing absolute path, try to navigate there first where supported.
                var suggestedPath = (pathTextBox.Text ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(suggestedPath) && Path.IsPathRooted(suggestedPath))
                {
                    try
                    {
                        var sf = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(suggestedPath);
                        if (sf is not null)
                        {
                            // Add to FutureAccessList so the picker may prefer it in some scenarios
                            var token = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(sf);
                        }
                    }
                    catch
                    {
                        // ignore - folder may not exist or be inaccessible; picker will open at default location
                    }
                }

                var folder = await picker.PickSingleFolderAsync();
                if (folder is not null)
                {
                    pathTextBox.Text = folder.Path ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                var err = new ContentDialog
                {
                    Title = "Folder picker error",
                    Content = $"Could not open folder picker: {ex.Message}",
                    PrimaryButtonText = "OK",
                    XamlRoot = root.XamlRoot
                };

                await err.ShowAsync();
            }
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var name = (nameTextBox.Text ?? string.Empty).Trim();
            var path = (pathTextBox.Text ?? string.Empty).Trim();

            // Final validation
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (!(Path.IsPathRooted(path) && Path.IsPathFullyQualified(path)))
            {
                var err = new ContentDialog
                {
                    Title = "Invalid path",
                    Content = "Please provide a valid absolute folder path.",
                    PrimaryButtonText = "OK",
                    XamlRoot = root.XamlRoot
                };

                await err.ShowAsync();
                return;
            }

            var nameExistsFinal = vm.Profiles.Any(p => string.Equals((p.Name ?? string.Empty).Trim(), name, StringComparison.OrdinalIgnoreCase));
            if (nameExistsFinal)
            {
                var err = new ContentDialog
                {
                    Title = "Duplicate name",
                    Content = "A profile with that name already exists.",
                    PrimaryButtonText = "OK",
                    XamlRoot = root.XamlRoot
                };

                await err.ShowAsync();
                return;
            }

            var pathExistsFinal = vm.Profiles.Any(p => string.Equals((p.Path ?? string.Empty).Trim(), path, StringComparison.OrdinalIgnoreCase));
            if (pathExistsFinal)
            {
                var err = new ContentDialog
                {
                    Title = "Duplicate path",
                    Content = "A profile with that folder path already exists.",
                    PrimaryButtonText = "OK",
                    XamlRoot = root.XamlRoot
                };

                await err.ShowAsync();
                return;
            }

            try
            {
                // Create directory if it doesn't exist (recursively)
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                var err = new ContentDialog
                {
                    Title = "Unable to create folder",
                    Content = $"Could not create folder '{path}': {ex.Message}",
                    PrimaryButtonText = "OK",
                    XamlRoot = root.XamlRoot
                };

                await err.ShowAsync();
                return;
            }

            var profile = new FSPSLibrary.Models.ProfileModel { Name = name, Path = path };
            Debug.WriteLine($"[DEBUG] Adding profile: Name='{profile.Name}', Path='{profile.Path}'");
            vm.AddProfile(profile);
        }

        return;
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
                Debug.WriteLine($"[DEBUG] Deleting profile: Name='{selected.Name}', Path='{selected.Path}'");
                vm.DeleteProfileCommand.Execute(null);
            }

            if (vm.ProfileSelectionChangedCommand.CanExecute(null))
            {
                vm.ProfileSelectionChangedCommand.Execute(null);
            }
        }
    }
}
