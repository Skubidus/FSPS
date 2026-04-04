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

    partial void OnNameChanged(string value)
    {
        OnPropertyChanged(nameof(CanOk));
    }

    [ObservableProperty]
    private string _path = string.Empty;

    partial void OnPathChanged(string value)
    {
        OnPropertyChanged(nameof(CanOk));
    }

    public string DialogTitle => _mode == DialogMode.Create ? "New Profile" : "Edit Profile";

    public bool CanOk => Validate(out _);

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
    }

    public bool Validate(out string? error)
    {
        var name = (Name ?? string.Empty).Trim();
        var pathInput = (Path ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            error = "Name must not be empty.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(pathInput))
        {
            error = "Path must not be empty.";
            return false;
        }

        // Normalize path (GetFullPath) for reliable comparisons. If invalid, report to user.
        static string NormalizeAndPreserveRoot(string raw)
        {
            // GetFullPath will resolve and normalize the path; keep the trailing separator for root drives (e.g. "C:\")
            var full = System.IO.Path.GetFullPath(raw.Trim());
            var root = System.IO.Path.GetPathRoot(full) ?? string.Empty;
            if (full.Length > root.Length)
            {
                // not a root-only path -> remove trailing separators
                return full.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
            }

            // keep root exactly as returned by GetFullPath (e.g. "C:\")
            return full;
        }

        string normalizedPath;
        try
        {
            normalizedPath = NormalizeAndPreserveRoot(pathInput);
        }
        catch (Exception)
        {
            error = "Path is not valid.";
            return false;
        }

        if (!System.IO.Path.IsPathRooted(normalizedPath) || !System.IO.Path.IsPathFullyQualified(normalizedPath))
        {
            error = "Path must be absolute.";
            return false;
        }

        // Helper to normalize stored profile paths for reliable comparisons
        static string? NormalizePathSafe(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            try
            {
                return NormalizeAndPreserveRoot(raw.Trim());
            }
            catch
            {
                return null; // treat invalid stored paths as absent for duplicate checks
            }
        }

        // Exclude the original profile from duplicate checks when editing.
        bool IsSameAsOriginalName = _originalProfile is not null && string.Equals((_originalProfile.Name ?? string.Empty).Trim(), name, StringComparison.OrdinalIgnoreCase);

        var originalPathNormalized = NormalizePathSafe(_originalProfile?.Path);
        bool IsSameAsOriginalPath = _originalProfile is not null && originalPathNormalized is not null && string.Equals(originalPathNormalized, normalizedPath, StringComparison.OrdinalIgnoreCase);

        if (_mode == DialogMode.Create || !IsSameAsOriginalName)
        {
            if (_profiles.Any(p => !ReferenceEquals(p, _originalProfile) && string.Equals((p.Name ?? string.Empty).Trim(), name, StringComparison.OrdinalIgnoreCase)))
            {
                error = "Name already exists.";
                return false;
            }
        }

        if (_mode == DialogMode.Create || !IsSameAsOriginalPath)
        {
            if (_profiles.Any(p =>
            {
                if (ReferenceEquals(p, _originalProfile))
                {
                    return false; // skip original
                }

                var pNorm = NormalizePathSafe(p.Path);
                if (pNorm is null)
                {
                    return false; // ignore invalid stored paths
                }

                return string.Equals(pNorm, normalizedPath, StringComparison.OrdinalIgnoreCase);
            }))
            {
                error = "Path already exists.";
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
