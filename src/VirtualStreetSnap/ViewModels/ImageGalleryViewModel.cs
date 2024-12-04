using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Services;
using VirtualStreetSnap.Views;

namespace VirtualStreetSnap.ViewModels;

public class LazyLoadManager
{
    private const int BatchSize = 20;
    private Dictionary<string, DateTime> _allImagePaths = new();
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

        _allImagePaths = Directory.GetFiles(saveDirectory, "*.png")
            .ToDictionary(file => file, File.GetLastWriteTime);
        _allImagePaths = _allImagePaths.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
        Thumbnails.Clear();
        _currentBatchIndex = 0;
        LoadNextBatch();
        IsInitialized = true;
    }

    public void LoadNextBatch()
    {
        var nextBatch = _allImagePaths.Skip(_currentBatchIndex * BatchSize).Take(BatchSize);
        foreach (var kv in nextBatch) Thumbnails.Add(new ImageBase(kv.Key));
        _currentBatchIndex++;
    }

    public void LoadNecessary()
    {
        var currentImagePaths = Directory.GetFiles(_lastCheckedDirectory, "*.png")
            .ToDictionary(file => file, File.GetLastWriteTime);

        // Detect and remove missing files
        var missingFiles = _allImagePaths.Keys.Except(currentImagePaths.Keys).ToList();
        foreach (var missingFile in missingFiles)
        {
            var imageToRemove = Thumbnails.FirstOrDefault(img => img.ImgPath == missingFile);
            if (imageToRemove != null)
            {
                Thumbnails.Remove(imageToRemove);
            }
            _allImagePaths.Remove(missingFile);
        }

        // Detect and add new files
        var newImagePaths = currentImagePaths.Keys.Except(_allImagePaths.Keys).Reverse().ToList();
        if (newImagePaths.Count == 0) return;

        foreach (var file in newImagePaths)
        {
            _allImagePaths[file] = currentImagePaths[file];
            Thumbnails.Insert(0, new ImageBase(file));
        }
    }
}

public partial class ImageGalleryViewModel : ViewModelBase
{
    private readonly LazyLoadManager _lazyLoadManager = new();

    [ObservableProperty]
    private bool _showColorPicker;

    [ObservableProperty]
    private ImageBase? _selectedThumbnail;

    [ObservableProperty]
    private AppConfig _config = ConfigService.Instance;

    [ObservableProperty]
    private ImageViewerView _selectedImageViewer;
    
    [ObservableProperty]
    private int _thumbDisplaySize = 80;
    
    public ObservableCollection<ImageBase> Thumbnails => _lazyLoadManager.Thumbnails;
    
    public ImageGalleryViewModel()
    {
        SelectedImageViewer = new ImageViewerView();
        UpdateThumbnails(selectFirst:false);
        Config.Settings.PropertyChanged += OnSettingsPropertyChanged;
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateThumbnails(true);
    }

    partial void OnSelectedThumbnailChanged(ImageBase value)
    {
        value.LoadImage();
        if (SelectedImageViewer.DataContext is ImageViewerViewModel viewModel) viewModel.ViewImage = value;
    }

    public void UpdateThumbnails(bool reload = false,bool selectFirst = true)
    {
        if (_lazyLoadManager.IsInitialized && !reload)
        {
            _lazyLoadManager.LoadNecessary();
        }
        else
        {
            _lazyLoadManager.Initialize(Config.Settings.SaveDirectory);
        }

        if (Thumbnails.Count > 0 && SelectedThumbnail == null && selectFirst) SelectedThumbnail = Thumbnails.First();
    }


    [RelayCommand]
    public void LoadMoreThumbnails()
    {
        _lazyLoadManager.LoadNextBatch();
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
    public void EditSelectedImage(Window window)
    {
        if (SelectedThumbnail == null) return;
        if (Design.IsDesignMode) return;

        var newImage = new ImageBase(SelectedThumbnail.ImgPath);
        newImage.LoadImage();
        
        var editorWindow = new ImageEditorView
        {
            DataContext = new ImageEditorViewModel(newImage)
        };
        var viewModel = editorWindow.DataContext as ImageEditorViewModel;
        viewModel.ImageSaved += (sender, args) =>
        {
            UpdateThumbnails(true);
            if (Thumbnails.Count > 0)
            {   
                SelectedThumbnail = Thumbnails.First();
            }
            GC.Collect();
        };
        editorWindow.Show();
    }
}