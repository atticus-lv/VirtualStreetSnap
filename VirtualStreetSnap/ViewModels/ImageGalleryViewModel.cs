using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels;

public partial class ImageGalleryViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<ImageThumbViewModel> _thumbnails = new();

    [ObservableProperty]
    private bool _showThumbnailBar = true;

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
        if (e.PropertyName == nameof(Settings.SaveDirectory)) LoadThumbnails(Config.Settings.SaveDirectory);
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

    [RelayCommand]
    public void ToggleThumbnailBar()
    {
        ShowThumbnailBar = !ShowThumbnailBar;
    }

    [RelayCommand]
    public void DeleteSelectedThumbnail()
    {
        if (SelectedThumbnail == null) return;
        var filePath = SelectedThumbnail.ImgPath;
        if (!File.Exists(filePath)) return;
        File.Delete(filePath);
        Thumbnails.Remove(SelectedThumbnail);
        SelectedThumbnail = null;
    }

    [RelayCommand]
    public void OpenSelectedThumbnailFolder()
    {
        if (SelectedThumbnail == null) return;
        var filePath = SelectedThumbnail.ImgPath;
        var folderPath = Path.GetDirectoryName(filePath);
        if (folderPath != null && Directory.Exists(folderPath))
            Process.Start(new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true,
                Verb = "open"
            });
    }
}