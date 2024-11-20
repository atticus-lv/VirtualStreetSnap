using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels;

public partial class ImageGalleryViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<ImageThumbViewModel> _thumbnails = new();

    [ObservableProperty]
    private ImageThumbViewModel? _selectedThumbnail;

    [ObservableProperty]
    private Bitmap? _selectedImage;
    
    [ObservableProperty]
    private string _selectedImageName = "";

    [ObservableProperty]
    private AppConfig _config = ConfigService.Instance;

    public ImageGalleryViewModel()
    {
        LoadThumbnails(Config.Settings.SaveDirectory);
        Config.Settings.PropertyChanged += OnSettingsPropertyChanged;
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Settings.SaveDirectory))
        {
            LoadThumbnails(Config.Settings.SaveDirectory);
        }
    }

    public void LoadThumbnails(string directoryPath)
    {
        if (!Directory.Exists(directoryPath)) return;
        var pngFiles = Directory.GetFiles(directoryPath, "*.png");
        Thumbnails.Clear();
        foreach (var file in pngFiles) Thumbnails.Add(new ImageThumbViewModel(file));
    }

    partial void OnSelectedThumbnailChanged(ImageThumbViewModel? value)
    {
        SelectedImage = value?.Image;
        SelectedImageName = value?.ImgName ?? "Unknown";
    }
}