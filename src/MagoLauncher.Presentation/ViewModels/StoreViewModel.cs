using CommunityToolkit.Mvvm.ComponentModel;
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

        private readonly HttpClient _httpClient;

        public StoreViewModel()
        {
            _httpClient = new HttpClient();
            _modpacks = new ObservableCollection<Modpack>();
            _ = LoadModpacks();
        }

        private async Task LoadModpacks()
        {
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
    }
}
