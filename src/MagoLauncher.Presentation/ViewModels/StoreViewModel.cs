using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagoLauncher.Presentation.Models;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using System.IO;
using System;

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

        public StoreViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _httpClient = new HttpClient();
            _modpacks = new ObservableCollection<Modpack>();
            _ = LoadModpacks();
        }

        public StoreViewModel()
        {
            // Design-time constructor
            // Avoid creating MainWindowViewModel here to prevent circular dependency/stack overflow
            _mainWindowViewModel = null!;
            _httpClient = new HttpClient();
            _modpacks = new ObservableCollection<Modpack>();
        }

        private async Task LoadModpacks()
        {
            IsLoading = true;
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:3000/modpacks");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var modpacks = JsonSerializer.Deserialize<ObservableCollection<Modpack>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (modpacks != null)
                {
                    Modpacks = modpacks;
                    foreach (var modpack in Modpacks)
                    {
                        _ = LoadThumbnail(modpack);
                    }
                }
            }
            catch (HttpRequestException)
            {
                // TODO: Handle exception
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
                // TODO: Handle image loading error
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
