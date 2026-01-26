using CommunityToolkit.Mvvm.ComponentModel;
using MagoLauncher.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MagoLauncher.Presentation.Services.Navigation;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private ViewModelBase _currentViewModel = null!;

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            if (_currentViewModel != value)
            {
                _currentViewModel = value;
                CurrentViewModelChanged?.Invoke(_currentViewModel);
            }
        }
    }

    public event Action<ViewModelBase>? CurrentViewModelChanged;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<T>() where T : ViewModelBase
    {
        var viewModel = _serviceProvider.GetRequiredService<T>();
        CurrentViewModel = viewModel;
    }

    public void NavigateTo<T>(object parameter) where T : ViewModelBase
    {
        // For parameterized ViewModels, we can use ActiviatorUtilities if they are not strictly registered as singletons, 
        // or we can rely on a Factory convention.
        // Usually, ModpackDetailViewModel takes 'Modpack' in constructor. 
        // We will assume that parameterized ViewModels are NOT registered in DI container as services, 
        // or they are created via specific Factory methods. 
        // But to keep it generic, let's try to resolve a Factory or use ActivatorUtilities.

        // Since we are refactoring, let's use ActivatorUtilities to create the instance with DI dependencies + parameters.
        try
        {
            var viewModel = ActivatorUtilities.CreateInstance<T>(_serviceProvider, parameter);
            CurrentViewModel = viewModel;
        }
        catch (Exception ex)
        {
            // Fallback or explicit error handling
            throw new InvalidOperationException($"Failed to navigate to {typeof(T).Name} with parameter.", ex);
        }
    }
}
