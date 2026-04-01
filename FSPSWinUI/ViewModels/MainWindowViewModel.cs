using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FSPSLibrary.Models;
using System.Collections.ObjectModel;

namespace FSPSWinUI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<Profile> Profiles { get; } = new ObservableCollection<Profile>();

    [ObservableProperty]
    private Profile? _selectedProfile;

    [ObservableProperty]
    private string _appTitle = "FSPS";

    public MainWindowViewModel()
    {
        // sample placeholder entries - can be removed later
        Profiles.Add(new Profile { Name = "Default" });
        Profiles.Add(new Profile { Name = "Modded" });
        _selectedProfile = Profiles.Count > 0 ? Profiles[0] : null;
    }

    [RelayCommand]
    private void AddProfile()
    {
        // TODO: Implement adding logic.
    }

    [RelayCommand]
    private void EditProfile()
    {
        // TODO: Implement edit logic.
    }

    [RelayCommand]
    private void DeleteProfile()
    {
        if (_selectedProfile is null) return;
        Profiles.Remove(_selectedProfile);
        _selectedProfile = Profiles.Count > 0 ? Profiles[0] : null;
    }
}
