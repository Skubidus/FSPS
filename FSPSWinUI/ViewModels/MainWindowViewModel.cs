using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FSPSLibrary.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;

namespace FSPSWinUI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{


    public ObservableCollection<ProfileModel> Profiles { get; } = new ObservableCollection<ProfileModel>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanDelete))]
    private ProfileModel? _selectedProfile;

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
        Profiles.Add(new ProfileModel { Name = "Default" });
        Profiles.Add(new ProfileModel { Name = "Modded" });
        Profiles.CollectionChanged += (s, e) => { OnPropertyChanged(nameof(CanDelete)); };
        SelectedProfile = Profiles.Count > 0 ? Profiles[0] : null;

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        _appTitle = config["App:Title"] ?? _appTitle;
    }

    [RelayCommand]
    private async void AddProfile()
    {
        var dialogVm = new ProfileDialogViewModel(Profiles, ProfileDialogViewModel.DialogMode.Create);
        var dialog = new FSPSWinUI.Views.ProfileDialog(dialogVm, App.MainWindow.Content.XamlRoot);
        var result = await dialog.ShowAsync();
        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            var profile = dialogVm.GetResult();
            try
            {
                if (!System.IO.Directory.Exists(profile.Path))
                {
                    System.IO.Directory.CreateDirectory(profile.Path);
                }
            }
            catch (Exception ex)
            {
                var err = new Microsoft.UI.Xaml.Controls.ContentDialog
                {
                    Title = "Unable to create folder",
                    Content = $"Could not create folder '{profile.Path}': {ex.Message}",
                    PrimaryButtonText = "OK",
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };
                await err.ShowAsync();
                return;
            }
            AddProfile(profile);
        }
    }

    [RelayCommand]
    private async void EditProfile()
    {
        if (SelectedProfile is null)
        {
            return;
        }
        var dialogVm = new ProfileDialogViewModel(Profiles, ProfileDialogViewModel.DialogMode.Edit, SelectedProfile);
        var dialog = new FSPSWinUI.Views.ProfileDialog(dialogVm, App.MainWindow.Content.XamlRoot);
        var result = await dialog.ShowAsync();
        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            var updated = dialogVm.GetResult();
            var oldName = SelectedProfile.Name;
            var oldPath = SelectedProfile.Path;
            SelectedProfile.Name = updated.Name;
            SelectedProfile.Path = updated.Path;
            Debug.WriteLine($"[INFO] Profile edited: OldName='{oldName}', OldPath='{oldPath}' → NewName='{updated.Name}', NewPath='{updated.Path}'");
            // Optionally, re-sort or trigger any update logic
        }
        return;
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
        Debug.WriteLine($"[INFO] Profile deleted: Name='{toDelete.Name}', Path='{toDelete.Path}'");
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

    public void AddProfile(ProfileModel profile)
    {
        if (profile is null)
        {
            return;
        }

        var currentSelectionName = SelectedProfile?.Name;
        Profiles.Add(profile);
        Debug.WriteLine($"[INFO] New profile created: Name='{profile.Name}', Path='{profile.Path}'");
        SortProfiles();

        var newSelected = Profiles.FirstOrDefault(p => object.ReferenceEquals(p, profile));
        if (newSelected is null && profile.Name is not null)
        {
            newSelected = Profiles.FirstOrDefault(p => string.Equals(p.Name, profile.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (newSelected is not null)
        {
            SelectedProfile = newSelected;
            Debug.WriteLine($"[INFO] Profile selected: Name='{newSelected.Name}', Path='{newSelected.Path}'");
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
