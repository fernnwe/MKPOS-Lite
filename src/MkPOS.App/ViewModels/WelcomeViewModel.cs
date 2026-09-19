using CommunityToolkit.Mvvm.ComponentModel;

namespace MKPOS.App.ViewModels;

public sealed class WelcomeViewModel : ObservableObject, IModuleViewModel
{
    public Task LoadAsync()
    {
        return Task.CompletedTask;
    }
}