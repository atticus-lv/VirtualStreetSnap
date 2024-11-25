using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels;

public class LazyLoadManager
{
    private const int BatchSize = 20;
    private List<string> _allImagePaths = new();
    private int _currentBatchIndex;
    private DateTime _lastCheckedTime = DateTime.MinValue;
    private string _lastCheckedDirectory = string.Empty;

    public ObservableCollection<ImageBaseViewModel> Thumbnails { get; } = new();

    public void Initialize(string saveDirectory)
    {
        if (!Directory.Exists(saveDirectory)) return;

        var lastWriteTime = Directory.GetLastWriteTime(saveDirectory);
        if (lastWriteTime <= _lastCheckedTime && saveDirectory == _lastCheckedDirectory) return;

        _lastCheckedTime = lastWriteTime;
        _lastCheckedDirectory = saveDirectory;

        _allImagePaths = Directory.GetFiles(saveDirectory, "*.png").ToList();
        _allImagePaths.Reverse();
        Thumbnails.Clear();
        _currentBatchIndex = 0;
        LoadNextBatch();
    }

    public void LoadNextBatch()
    {
        var nextBatch = _allImagePaths.Skip(_currentBatchIndex * BatchSize).Take(BatchSize);
        foreach (var file in nextBatch) Thumbnails.Add(new ImageBaseViewModel(file));
        _currentBatchIndex++;
    }
}

public partial class ImageGalleryViewModel : ViewModelBase
{
    private readonly LazyLoadManager _lazyLoadManager = new();

    [ObservableProperty]
    private bool _showColorPicker;

    [ObservableProperty]
    private bool _showThumbnailBar = true;

    [ObservableProperty]
    private ImageBaseViewModel? _selectedThumbnail;

    [ObservableProperty]
    private AppConfig _config = ConfigService.Instance;

    public ObservableCollection<ImageBaseViewModel> Thumbnails => _lazyLoadManager.Thumbnails;

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
        _lazyLoadManager.Initialize(Config.Settings.SaveDirectory);
        if (Thumbnails.Count > 0 && SelectedThumbnail == null) SelectedThumbnail = Thumbnails[0];
    }

    [RelayCommand]
    public void LoadMoreThumbnails()
    {
        _lazyLoadManager.LoadNextBatch();
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
    public void SelectNextThumbnail()
    {
        if (SelectedThumbnail == null) return;
        var index = Thumbnails.IndexOf(SelectedThumbnail);
        if (index < Thumbnails.Count - 1) SelectedThumbnail = Thumbnails[index + 1];
    }

    [RelayCommand]
    public void SelectPreviousThumbnail()
    {
        if (SelectedThumbnail == null) return;
        var index = Thumbnails.IndexOf(SelectedThumbnail);
        if (index > 0) SelectedThumbnail = Thumbnails[index - 1];
    }

    [RelayCommand]
    public async Task CopySelectedImage()
    {
        if (SelectedThumbnail == null) return;
        await PowerShellClipBoard.SetImage(SelectedThumbnail.ImgPath);
    }
}