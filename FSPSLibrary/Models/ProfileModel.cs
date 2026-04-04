using CommunityToolkit.Mvvm.ComponentModel;

namespace FSPSLibrary.Models;

public sealed partial class ProfileModel : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string path = string.Empty;
}
