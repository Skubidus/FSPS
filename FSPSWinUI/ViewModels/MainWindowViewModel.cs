using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FSPSLibrary;
using FSPSLibrary.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace FSPSWinUI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{


    public ObservableCollection<ProfileModel> Profiles { get; } = new ObservableCollection<ProfileModel>();

    // Indicates whether there are any profiles present. UI binds to this to enable/disable
    // controls that require at least one profile (e.g., the profile ComboBox).
    public bool HasProfiles => Profiles.Count > 0;

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

    private readonly IProfileStore _profileStore;

    public MainWindowViewModel()
    {
        // initialize profile store pointing to profiles.json in the app folder
        _profileStore = new FSPSWinUI.Storage.JsonProfileStore(Path.Combine(AppContext.BaseDirectory, "profiles.json"));

        // load existing profiles from store (synchronous in ctor to ensure UI shows them immediately)
        try
        {
            var loaded = _profileStore.LoadAsync().GetAwaiter().GetResult();
            foreach (var p in loaded)
            {
                Profiles.Add(p);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[WARN] Unable to load profiles: {ex}");
        }

        // keep CanDelete/HasProfiles updated and persist changes when collection changes
        Profiles.CollectionChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(CanDelete));
            OnPropertyChanged(nameof(HasProfiles));
            _ = SaveProfilesAsync(); // fire-and-forget; errors logged inside SaveProfilesAsync
        };

        SelectedProfile = Profiles.Count > 0 ? Profiles[0] : null;

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        _appTitle = config["App:Title"] ?? _appTitle;
    }

    [RelayCommand]
    private async Task AddProfileAsync()
    {
        var dialogVm = new ProfileDialogViewModel(Profiles, ProfileDialogViewModel.DialogMode.Create);
        var dialog = new FSPSWinUI.Views.ProfileDialog(dialogVm, App.MainWindow!.Content.XamlRoot);
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

            await AddProfileAsync(profile);
        }
    }

    [RelayCommand]
    private async Task EditProfileAsync()
    {
        if (SelectedProfile is null)
        {
            return;
        }
        var dialogVm = new ProfileDialogViewModel(Profiles, ProfileDialogViewModel.DialogMode.Edit, SelectedProfile);
        var dialog = new FSPSWinUI.Views.ProfileDialog(dialogVm, App.MainWindow!.Content.XamlRoot);
        var result = await dialog.ShowAsync();
        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            var updated = dialogVm.GetResult();
            var oldName = SelectedProfile.Name;
            var oldPath = SelectedProfile.Path;

            // Ensure the new path exists (like AddProfile)
            try
            {
                if (!System.IO.Directory.Exists(updated.Path))
                {
                    System.IO.Directory.CreateDirectory(updated.Path);
                }
            }
            catch (Exception ex)
            {
                var err = new Microsoft.UI.Xaml.Controls.ContentDialog
                {
                    Title = "Unable to create folder",
                    Content = $"Could not create folder '{updated.Path}': {ex.Message}",
                    PrimaryButtonText = "OK",
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };
                await err.ShowAsync();
                return;
            }

            SelectedProfile.Name = updated.Name;
            SelectedProfile.Path = updated.Path;
            Debug.WriteLine($"[INFO] Profile edited: OldName='{oldName}', OldPath='{oldPath}' → NewName='{updated.Name}', NewPath='{updated.Path}'");
            // Re-sort collection and persist changes to the store
            try
            {
                SortProfiles();
                await SaveProfilesAsync().ConfigureAwait(false);
                Debug.WriteLine("[INFO] Profiles saved to store after edit.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Failed to save profiles after edit: {ex}");
            }
        }
        return;
    }

    [RelayCommand]
    private async Task DeleteProfile()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        var toDelete = SelectedProfile;
        Profiles.Remove(toDelete);
        Debug.WriteLine($"[INFO] Profile deleted: Name='{toDelete.Name}', Path='{toDelete.Path}'");
        SelectedProfile = Profiles.Count > 0 ? Profiles[0] : null;

        try
        {
            await SaveProfilesAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] Failed to save profiles after delete: {ex}");
        }

        return;
    }

    public async Task AddProfileAsync(ProfileModel profile)
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

        try
        {
            await SaveProfilesAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] Failed to save profiles after add: {ex}");
        }

        OnPropertyChanged(nameof(CanDelete));
        OnPropertyChanged(nameof(HasProfiles));

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

    private async Task SaveProfilesAsync()
    {
        try
        {
            await _profileStore.SaveAsync(Profiles).ConfigureAwait(false);
            Debug.WriteLine("[INFO] Profiles saved to store.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] Failed to save profiles: {ex}");
        }
    }

    [RelayCommand]
    private void ProfileSelectionChanged()
    {
        // Placeholder for logic after profile selection changes
        return;
    }
}
