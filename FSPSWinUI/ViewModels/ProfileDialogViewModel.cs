using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FSPSLibrary.Models;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace FSPSWinUI.ViewModels;

public partial class ProfileDialogViewModel : ObservableObject
{
    public enum DialogMode { Create, Edit }

    private readonly ObservableCollection<ProfileModel> _profiles;
    private readonly ProfileModel? _originalProfile;
    private readonly DialogMode _mode;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _path = string.Empty;

    public string DialogTitle => _mode == DialogMode.Create ? "Neues Profil" : "Profil bearbeiten";

    public IRelayCommand BrowseCommand { get; }

    public ProfileDialogViewModel(ObservableCollection<ProfileModel> profiles, DialogMode mode, ProfileModel? profileToEdit = null)
    {
        _profiles = profiles;
        _mode = mode;
        _originalProfile = profileToEdit;
        if (mode == DialogMode.Edit && profileToEdit is not null)
        {
            Name = profileToEdit.Name;
            Path = profileToEdit.Path;
        }
        // BrowseCommand is set in dialog code-behind if needed
    }

    private void OnBrowse()
    {
        // Browse logic will be handled in dialog code-behind (to access XamlRoot/window handle)
        return;
    }

    public bool Validate(out string? error)
    {
        var name = (Name ?? string.Empty).Trim();
        var path = (Path ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            error = "Name darf nicht leer sein.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(path))
        {
            error = "Pfad darf nicht leer sein.";
            return false;
        }
        if (!System.IO.Path.IsPathRooted(path) || !System.IO.Path.IsPathFullyQualified(path))
        {
            error = "Pfad muss absolut sein.";
            return false;
        }
        if (_mode == DialogMode.Create || (name != _originalProfile?.Name))
        {
            if (_profiles.Any(p => string.Equals((p.Name ?? string.Empty).Trim(), name, StringComparison.OrdinalIgnoreCase)))
            {
                error = "Name existiert bereits.";
                return false;
            }
        }
        if (_mode == DialogMode.Create || (path != _originalProfile?.Path))
        {
            if (_profiles.Any(p => string.Equals((p.Path ?? string.Empty).Trim(), path, StringComparison.OrdinalIgnoreCase)))
            {
                error = "Pfad existiert bereits.";
                return false;
            }
        }
        error = null;
        return true;
    }

    public ProfileModel GetResult()
    {
        return new ProfileModel { Name = Name.Trim(), Path = Path.Trim() };
    }
}
