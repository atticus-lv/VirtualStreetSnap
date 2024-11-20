﻿using System.Collections.Generic;
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
        _allImagePaths = Directory.GetFiles(directoryPath, "*.png").ToList();
        Thumbnails.Clear();
        _currentBatchIndex = 0;
        LoadNextBatch();
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