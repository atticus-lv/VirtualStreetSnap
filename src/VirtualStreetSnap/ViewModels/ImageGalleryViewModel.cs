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
using Avalonia.Threading;

namespace VirtualStreetSnap.ViewModels;

public partial class ImageGalleryViewModel : ViewModelBase, IDisposable
{
    private readonly LazyLoadManager _lazyLoadManager = new();
    private readonly FileSystemWatcher _watcher = new();
    private readonly Dictionary<string, DateTime> _imageFiles = new();

    [ObservableProperty]
    private bool _showColorPicker;

    [ObservableProperty]
    private ImageModelBase? _selectedThumbnail;

    [ObservableProperty]
    private AppConfig _config = ConfigService.Instance;

    [ObservableProperty]
    private ImageViewerView _selectedImageViewer;

    [ObservableProperty]
    private int _thumbDisplaySize = 80;

    [ObservableProperty]
    private ImageEditorWindow? _editorWindow;

    public ObservableCollection<ImageModelBase> Thumbnails => _lazyLoadManager.Thumbnails;

    public ImageGalleryViewModel()
    {
        SelectedImageViewer = new ImageViewerView();
        InitializeFileWatcher();
        UpdateThumbnails(selectFirst: false);
        Config.Settings.PropertyChanged += OnSettingsPropertyChanged;
    }

    private void InitializeFileWatcher()
    {
        _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
        _watcher.Filter = "*.png";
        _watcher.Created += OnFileChanged;
        _watcher.Deleted += OnFileChanged;
        _watcher.Changed += OnFileChanged;
        _watcher.Renamed += OnFileRenamed;
        
        if(Directory.Exists(Config.Settings.SaveDirectory))
        {
            _watcher.Path = Config.Settings.SaveDirectory;
            _watcher.EnableRaisingEvents = true;
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if(!File.Exists(e.FullPath)) return;
        
        Dispatcher.UIThread.Post(() =>
        {
            UpdateThumbnails(true);
        });
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            UpdateThumbnails(true); 
        });
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateThumbnails(true);
    }

    partial void OnSelectedThumbnailChanged(ImageModelBase value)
    {
        value.LoadImageAsync();
        if (SelectedImageViewer.DataContext is ImageViewerViewModel viewModel) viewModel.ViewImage = value;
    }

    public void UpdateThumbnails(bool reload = false, bool selectFirst = true)
    {
        var saveDirectory = Config.Settings.SaveDirectory;
        if (!Directory.Exists(saveDirectory)) return;

        if(_watcher.Path != saveDirectory)
        {
            _watcher.Path = saveDirectory;
            _watcher.EnableRaisingEvents = true;
        }

        if(reload)
        {
            _imageFiles.Clear();
            var files = Directory.GetFiles(saveDirectory, "*.png");
            foreach(var file in files)
            {
                _imageFiles[file] = File.GetLastWriteTime(file);
            }
            _lazyLoadManager.Initialize(saveDirectory);
        }
        else if(!_lazyLoadManager.IsInitialized)
        {
            _lazyLoadManager.Initialize(saveDirectory);
        }
        
        if (Thumbnails.Count > 0 && SelectedThumbnail == null && selectFirst)
        {
            SelectedThumbnail = Thumbnails.First();
        }
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
        var folderPath = Path.GetDirectoryName(filePath as string);
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

    public void CreateEditorWindow()
    {
        if (EditorWindow is not null) return;
        EditorWindow = new ImageEditorWindow()
        {
            DataContext = new ImageEditorWindowViewModel()
        };
    }

    [RelayCommand]
    public void EditSelectedImage(Window window)
    {
        if (SelectedThumbnail == null) return;
        if (Design.IsDesignMode) return;

        var newImage = new ImageModelBase(SelectedThumbnail.ImgPath);
        newImage.LoadImageAsync();

        if (EditorWindow is not null)
        {
            var viewModel = EditorWindow.DataContext as ImageEditorWindowViewModel;
            viewModel?.AddPage(newImage);
        }
        else
        {
            CreateEditorWindow();
            var viewModel = EditorWindow.DataContext as ImageEditorWindowViewModel;
            viewModel?.AddPage(newImage);
            viewModel.ImageSaved += (sender, args) =>
            {
                UpdateThumbnails(true);
                if (Thumbnails.Count > 0)
                {
                    SelectedThumbnail = Thumbnails.First();
                }
            };
        }

        EditorWindow.Show();
        EditorWindow.Activate();
    }

    public void Dispose()
    {
        Config.Settings.PropertyChanged -= OnSettingsPropertyChanged;
        _watcher.Dispose();
    }
}