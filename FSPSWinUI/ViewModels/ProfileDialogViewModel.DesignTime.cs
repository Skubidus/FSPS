using FSPSLibrary.Models;
using System.Collections.ObjectModel;

namespace FSPSWinUI.ViewModels;

public class ProfileDialogViewModelDesignTime : ProfileDialogViewModel
{
    public ProfileDialogViewModelDesignTime() : base(
        new ObservableCollection<ProfileModel> {
            new ProfileModel { Name = "Default", Path = "C:\\Games\\FSPS\\Default" },
            new ProfileModel { Name = "Modded", Path = "C:\\Games\\FSPS\\Modded" }
        },
        DialogMode.Create)
    {
        Name = "Test";
        Path = "C:\\Games\\FSPS\\Test";
    }
}
