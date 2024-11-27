using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Services;
using VirtualStreetSnap.Views;

namespace VirtualStreetSnap.ViewModels;

public class LazyLoadManager
{
    private const int BatchSize = 20;
    private List<string> _allImagePaths = new();
    private int _currentBatchIndex;
    private DateTime _lastCheckedTime = DateTime.MinValue;
    private string _lastCheckedDirectory = string.Empty;

    public bool IsInitialized;

    public ObservableCollection<ImageBase> Thumbnails { get; } = new();

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
        IsInitialized = true;
    }

    public void LoadNextBatch()
    {
        var nextBatch = _allImagePaths.Skip(_currentBatchIndex * BatchSize).Take(BatchSize);
        foreach (var file in nextBatch) Thumbnails.Add(new ImageBase(file));
        _currentBatchIndex++;
    }

    public void LoadNecessary()
    {
        var newImagePaths = Directory.GetFiles(_lastCheckedDirectory, "*.png")
            .Except(_allImagePaths)
            .Reverse()
            .ToList();

        if (newImagePaths.Count == 0) return;

        _allImagePaths.InsertRange(0, newImagePaths);
        foreach (var file in newImagePaths) Thumbnails.Insert(0, new ImageBase(file));
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
    private Bitmap _selectedImage;

    [ObservableProperty]
    private ImageBase? _selectedThumbnail;

    [ObservableProperty]
    private AppConfig _config = ConfigService.Instance;

    [ObservableProperty]
    private ImageViewerViewModel _selectedImageViewer = new ImageViewerViewModel();

    public ObservableCollection<ImageBase> Thumbnails => _lazyLoadManager.Thumbnails;

    public ImageGalleryViewModel()
    {
        UpdateThumbnails();
        Config.Settings.PropertyChanged += OnSettingsPropertyChanged;
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateThumbnails(true);
    }

    partial void OnSelectedThumbnailChanged(ImageBase value)
    {
        value.LoadImage();
        SelectedImageViewer.ViewImage = value;
    }

    [RelayCommand]
    public void UpdateThumbnails(bool reload = false)
    {
        if (_lazyLoadManager.IsInitialized && !reload)
        {
            _lazyLoadManager.LoadNecessary();
        }
        else
        {
            _lazyLoadManager.Initialize(Config.Settings.SaveDirectory);
            if (Thumbnails.Count > 0 && SelectedThumbnail == null) SelectedThumbnail = Thumbnails[0];
        }
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
    
    [RelayCommand]
    public void EditSelectedImage()
    {
        if (SelectedThumbnail == null) return;
        if (Design.IsDesignMode) return;

        var editorWindow = new ImageEditorView
        {
            DataContext = new ImageEditorViewModel(SelectedThumbnail)
        };
        editorWindow.Show();
    }
}