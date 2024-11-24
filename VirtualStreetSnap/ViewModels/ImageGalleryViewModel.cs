using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels;

public partial class ImageGalleryViewModel : ViewModelBase
{
    private const int BatchSize = 20;
    private List<string> _allImagePaths = new();
    private int _currentBatchIndex;
    private DateTime _lastCheckedTime = DateTime.MinValue;
    private string _lastCheckedDirectory = string.Empty;

    [ObservableProperty]
    private bool _showColorPicker;

    [ObservableProperty]
    private bool _showThumbnailBar = true;

    [ObservableProperty]
    private ObservableCollection<ImageThumbViewModel> _thumbnails = new();

    [ObservableProperty]
    private ImageThumbViewModel? _selectedThumbnail;

    [ObservableProperty]
    private Bitmap? _selectedImage;

    [ObservableProperty]
    private string _selectedImageName = string.Empty;

    [ObservableProperty]
    private AppConfig _config = ConfigService.Instance;

    public ImageGalleryViewModel()
    {
        ReLoadThumbnails();
        Config.Settings.PropertyChanged += OnSettingsPropertyChanged;
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        ReLoadThumbnails();
    }

    [RelayCommand]
    public void ReLoadThumbnails()
    {
        if (!Directory.Exists(Config.Settings.SaveDirectory)) return;

        var lastWriteTime = Directory.GetLastWriteTime(Config.Settings.SaveDirectory);
        if (lastWriteTime <= _lastCheckedTime && Config.Settings.SaveDirectory == _lastCheckedDirectory) return;

        _lastCheckedTime = lastWriteTime;
        _lastCheckedDirectory = Config.Settings.SaveDirectory;

        _allImagePaths = Directory.GetFiles(Config.Settings.SaveDirectory, "*.png").ToList();
        _allImagePaths.Reverse();
        Thumbnails.Clear();
        _currentBatchIndex = 0;
        LoadNextBatch();
        if (Thumbnails.Count > 0 && SelectedThumbnail == null) SelectedThumbnail = Thumbnails[0];
    }

    private void LoadNextBatch()
    {
        var nextBatch = _allImagePaths.Skip(_currentBatchIndex * BatchSize).Take(BatchSize);
        foreach (var file in nextBatch) Thumbnails.Add(new ImageThumbViewModel(file));
        _currentBatchIndex++;
    }

    [RelayCommand]
    public void LoadMoreThumbnails()
    {
        if (_currentBatchIndex * BatchSize < _allImagePaths.Count) LoadNextBatch();
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

    [RelayCommand]
    public async void CopySelectedImage()
    {
        if (SelectedThumbnail == null) return;
        await PowerShellClipBoard.SetImage(SelectedThumbnail.ImgPath);
    }
    
}