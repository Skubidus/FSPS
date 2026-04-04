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
        // Validate on primary click; do not call Hide() here — let ContentDialog set the result to Primary.
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
        if (!ViewModel.Validate(out var error))
        {
            args.Cancel = true;
            var errorDialog = new ContentDialog
            {
                Title = "Error",
                Content = error,
                PrimaryButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            _ = errorDialog.ShowAsync();
            return;
        }
        // Let the ContentDialog complete normally so ShowAsync() returns Primary
        return;
    }
}
