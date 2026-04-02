using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace FSPSWinUI;

public sealed partial class MainWindow : Window
{
    private const int SM_CYCAPTION = 4;

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    public MainWindow()
    {
        InitializeComponent();

        // Extend content into the title bar and use the named draggable area for the caption drag region.
        this.ExtendsContentIntoTitleBar = true;

        // Determine system caption/button height and size the title bar to match.
        var captionHeight = GetSystemMetrics(SM_CYCAPTION);
        if (captionHeight > 0)
        {
            // Small negative offset to better match system caption button height on some systems
            TitleBarBorder.Height = Math.Max(1, captionHeight - 13);
        }

        this.SetTitleBar(TitleBarBorder);

        // Keep title bar theme-controlled
        var appWindow = this.AppWindow;
        appWindow.TitleBar.InactiveBackgroundColor = null; // keep theme resources
        appWindow.TitleBar.BackgroundColor = null;

        AppWindow.Resize(new Windows.Graphics.SizeInt32(1920, 1080));


        // Window.Title doesn't support data binding — sync it from the ViewModel after XAML is loaded.
        var vm = new ViewModels.MainWindowViewModel();
        if (this.Content is FrameworkElement root)
        {
            root.DataContext = vm;
        }

        this.Title = vm.AppTitle;
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModels.MainWindowViewModel.AppTitle))
            {
                this.Title = vm.AppTitle;
            }
        };

        return;
    }

    private async void AddProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (!(this.Content is FrameworkElement root && root.DataContext is ViewModels.MainWindowViewModel vm))
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
        if (!(this.Content is FrameworkElement root && root.DataContext is ViewModels.MainWindowViewModel vm))
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
