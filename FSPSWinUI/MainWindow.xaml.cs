namespace FSPSWinUI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        AppWindow.Resize(new Windows.Graphics.SizeInt32(1920, 1080));

        // Window.Title doesn't support data binding — sync it from the ViewModel after XAML is loaded.
        if (this.Content is FrameworkElement { DataContext: ViewModels.MainWindowViewModel vm })
        {
            this.Title = vm.AppTitle;
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModels.MainWindowViewModel.AppTitle))
                    this.Title = vm.AppTitle;
            };
        }
    }
}
