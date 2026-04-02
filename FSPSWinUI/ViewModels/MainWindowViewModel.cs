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
    [NotifyPropertyChangedFor(nameof(CanDelete))]
    private Profile? _selectedProfile;

    public bool CanDelete
    {
        get
        {
            return SelectedProfile != null;
        }
    }

    [ObservableProperty]
    private string _appTitle = "FSPS";

    public MainWindowViewModel()
    {
        // sample placeholder entries - can be removed later
        Profiles.Add(new Profile { Name = "Default" });
        Profiles.Add(new Profile { Name = "Modded" });
        Profiles.CollectionChanged += (s, e) => { OnPropertyChanged(nameof(CanDelete)); };
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

        var toDelete = SelectedProfile;
        Profiles.Remove(toDelete);
        if (Profiles.Count > 0)
        {
            SelectedProfile = Profiles[0];
        }
        else
        {
            SelectedProfile = null;
        }

        return;
    }

    public void AddProfile(Profile profile)
    {
        if (profile is null)
        {
            return;
        }

        var currentSelectionName = SelectedProfile?.Name;
        Profiles.Add(profile);
        SortProfiles();

        var newSelected = Profiles.FirstOrDefault(p => object.ReferenceEquals(p, profile));
        if (newSelected is null && profile.Name is not null)
        {
            newSelected = Profiles.FirstOrDefault(p => string.Equals(p.Name, profile.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (newSelected is not null)
        {
            SelectedProfile = newSelected;
        }
        else if (currentSelectionName is not null)
        {
            SelectedProfile = Profiles.FirstOrDefault(p => string.Equals(p.Name, currentSelectionName, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            SelectedProfile = Profiles.Count > 0 ? Profiles[0] : null;
        }

        return;
    }

    private void SortProfiles()
    {
        var selectedName = SelectedProfile?.Name;
        var sorted = Profiles.OrderBy(p => (p.Name ?? string.Empty), StringComparer.OrdinalIgnoreCase).ToList();
        Profiles.Clear();
        foreach (var p in sorted)
        {
            Profiles.Add(p);
        }

        if (selectedName is not null)
        {
            SelectedProfile = Profiles.FirstOrDefault(p => string.Equals(p.Name, selectedName, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            SelectedProfile = Profiles.Count > 0 ? Profiles[0] : null;
        }
    }

    [RelayCommand]
    private void ProfileSelectionChanged()
    {
        // Placeholder for logic after profile selection changes
        return;
    }
}
