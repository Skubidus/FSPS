using FSPSWinUI.ViewModels;

using Windows.Storage.Pickers;

using WinRT.Interop;

namespace FSPSWinUI.Views;

public sealed partial class ProfileDialog : ContentDialog
{
    public ProfileDialog(ProfileDialogViewModel viewModel, XamlRoot xamlRoot)
    {
        this.InitializeComponent();
        this.XamlRoot = xamlRoot;
        this.DataContext = viewModel;
        this.ViewModel = viewModel;
        this.PrimaryButtonClick += OnPrimaryButtonClick;
    }

    public ProfileDialogViewModel ViewModel { get; }

    private async void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(picker, hwnd);
        picker.FileTypeFilter.Add("*");
        var folder = await picker.PickSingleFolderAsync();
        if (folder is not null)
        {
            ViewModel.Path = folder.Path ?? string.Empty;
        }
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Ensure latest UI values are committed to the ViewModel before validation
        ViewModel.Name = NameTextBox.Text ?? string.Empty;
        ViewModel.Path = PathTextBox.Text ?? string.Empty;

        if (!ViewModel.Validate(out var error))
        {
            args.Cancel = true;
            ErrorTextBlock.Text = error ?? "Validation failed.";
            ErrorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            _ = NameTextBox.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
            return;
        }

        // hide any previous error
        ErrorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        return;
    }
}