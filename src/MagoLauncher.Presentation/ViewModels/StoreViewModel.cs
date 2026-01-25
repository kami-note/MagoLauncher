using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagoLauncher.Presentation.Models;
using MagoLauncher.Application.Services;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MagoLauncher.Presentation.ViewModels
{
    public partial class StoreViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<Modpack> _modpacks;

        [ObservableProperty]
        private bool _isLoading;

        private readonly HttpClient _httpClient;

        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly INotificationService _notificationService;
        private readonly IMinecraftInstanceService _instanceService;

        public StoreViewModel(MainWindowViewModel mainWindowViewModel, INotificationService notificationService, IMinecraftInstanceService instanceService)
        {
            _notificationService = notificationService;
            _mainWindowViewModel = mainWindowViewModel;
            _instanceService = instanceService;
            _httpClient = new HttpClient();
            _modpacks = new ObservableCollection<Modpack>();
            _ = LoadModpacks();
        }

        public StoreViewModel()
        {
            // Design-time constructor
            _mainWindowViewModel = null!;
            _notificationService = null!;
            _instanceService = null!;
            _httpClient = new HttpClient();
            _modpacks = new ObservableCollection<Modpack>();
        }

        [ObservableProperty]
        private bool _isOffline;

        [RelayCommand]
        public async Task Retry()
        {
            await LoadModpacks();
        }

        private async Task LoadModpacks()
        {
            IsLoading = true;
            IsOffline = false;
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:3000/modpacks");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var modpacks = JsonSerializer.Deserialize<ObservableCollection<Modpack>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (modpacks != null)
                {
                    // Check installed instances
                    IEnumerable<MagoLauncher.Domain.Entities.MinecraftInstance> instances = new List<MagoLauncher.Domain.Entities.MinecraftInstance>();
                    try
                    {
                        instances = await _instanceService.GetAllInstancesAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"Error fetching instances: {ex.Message}");
                    }

                    foreach (var modpack in modpacks)
                    {
                        var installedInstance = instances.FirstOrDefault(i =>
                            string.Equals(i.Metadata?.Slug, modpack.Slug, StringComparison.OrdinalIgnoreCase));

                        if (installedInstance != null)
                        {
                            modpack.IsInstalled = true;
                            modpack.InstanceId = installedInstance.Id;
                        }
                    }

                    Modpacks = modpacks;
                    foreach (var modpack in Modpacks)
                    {
                        _ = LoadThumbnail(modpack);
                    }
                }
            }
            catch (Exception ex)
            {
                IsOffline = true;
                _notificationService?.ShowError("Erro ao carregar loja", $"Não foi possível conectar ao servidor: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadThumbnail(Modpack modpack)
        {
            if (string.IsNullOrEmpty(modpack.Thumbnail))
                return;

            try
            {
                var imageData = await _httpClient.GetByteArrayAsync(modpack.Thumbnail);
                using (var stream = new MemoryStream(imageData))
                {
                    modpack.ThumbnailBitmap = new Bitmap(stream);
                }
            }
            catch (Exception)
            {
                // Silent failure for thumbnails is acceptable, or log we can show a warning
                // _notificationService?.ShowWarning("Aviso", $"Falha ao carregar imagem: {modpack.Name}");
            }
        }

        [RelayCommand]
        public void OpenModpackDetails(Modpack modpack)
        {
            if (modpack == null) return;
            _mainWindowViewModel.GoToModpackDetails(modpack);
        }
    }
}
