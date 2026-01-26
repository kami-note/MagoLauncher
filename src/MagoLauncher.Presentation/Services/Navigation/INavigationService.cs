using CommunityToolkit.Mvvm.ComponentModel;
using MagoLauncher.Presentation.ViewModels;
using System;

namespace MagoLauncher.Presentation.Services.Navigation;

public interface INavigationService
{
    ViewModelBase CurrentViewModel { get; }
    event Action<ViewModelBase> CurrentViewModelChanged;

    void NavigateTo<T>() where T : ViewModelBase;
    void NavigateTo<T>(object parameter) where T : ViewModelBase;
}
