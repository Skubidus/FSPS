using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FSPSLibrary.Models;
using Microsoft.Extensions.Configuration;
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
        SelectedProfile = Profiles.Count > 0 ? Profiles[0] : null;

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        _appTitle = config["App:Title"] ?? _appTitle;
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
        if (SelectedProfile is null)
        {
            return;
        }
        Profiles.Remove(SelectedProfile);
        SelectedProfile = Profiles.Count > 0 ? Profiles[0] : null;
    }
}
