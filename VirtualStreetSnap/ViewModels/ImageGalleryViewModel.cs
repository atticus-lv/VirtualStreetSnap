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
    // The number of thumbnails to load at a time
    private const int BatchSize = 20;

    // A list of all the image paths in the save directory
    private List<string> _allImagePaths = [];
    private int _currentBatchIndex;
    private DateTime _lastCheckedTime = DateTime.MinValue;

    [ObservableProperty]
    private bool _showThumbnailBar = true;

    // A collection of ImageThumbViewModels representing the thumbnails
    [ObservableProperty]
    private ObservableCollection<ImageThumbViewModel> _thumbnails = [];


    [ObservableProperty]
    private ImageThumbViewModel? _selectedThumbnail;

    [ObservableProperty]
    private Bitmap? _selectedImage;

    [ObservableProperty]
    private string _selectedImageName = "";

    // The current configuration of the app, use for getting the save directory
    [ObservableProperty]
    private AppConfig _config = ConfigService.Instance;

    public ImageGalleryViewModel()
    {
        ReLoadThumbnails();
        Config.Settings.PropertyChanged += OnSettingsPropertyChanged;
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Settings.SaveDirectory)) ReLoadThumbnails();
    }

    [RelayCommand]
    public void ReLoadThumbnails()
    {
        if (!Directory.Exists(Config.Settings.SaveDirectory)) return;

        var lastWriteTime = Directory.GetLastWriteTime(Config.Settings.SaveDirectory);
        if (lastWriteTime <= _lastCheckedTime) return;
        _lastCheckedTime = lastWriteTime;

        _allImagePaths = Directory.GetFiles(Config.Settings.SaveDirectory, "*.png").ToList();
        _allImagePaths.Reverse(); // reverse the list so the newest images are at the top
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

    // Commands for context menu
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