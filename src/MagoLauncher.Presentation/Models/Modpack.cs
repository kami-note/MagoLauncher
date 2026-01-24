using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MagoLauncher.Presentation.Models
{
    public class Modpack : ObservableObject
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public string? Version { get; set; }
        public string? MinecraftVersion { get; set; }
        public string? Author { get; set; }
        public string? Slug { get; set; }
        public int Downloads { get; set; }
        public string? Thumbnail { get; set; }
        public string? DownloadLink { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        private Bitmap? _thumbnailBitmap;
        public Bitmap? ThumbnailBitmap
        {
            get => _thumbnailBitmap;
            set => SetProperty(ref _thumbnailBitmap, value);
        }

        public System.Collections.Generic.List<Application.DTOs.ModpackChangelogDto>? Changelogs { get; set; }
    }
}
