using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Services;
using VirtualStreetSnap.ViewModels.ImageEditorLayer;

namespace VirtualStreetSnap.ViewModels;

public partial class ImageEditorViewModel : ViewModelBase
{
    private const string DefaultImagePath = "avares://VirtualStreetSnap/Assets/avalonia-logo.ico";

    [ObservableProperty]
    private ImageViewerViewModel _editImageViewer = new();

    [ObservableProperty]
    private LayerManagerViewModel _layerManager = new();

    [ObservableProperty]
    private LayerBaseViewModel? _selectedLayer;

    [ObservableProperty]
    private string? _dragItemText;

    [ObservableProperty]
    private bool _isDirty;

    public ObservableCollection<LayerTypeItem> LayerTypes { get; set; }

    public ImageEditorViewModel(ImageModelBase? image)
    {
        LayerTypes = new ObservableCollection<LayerTypeItem>(
            _layerConstructors.Keys.Select(key => new LayerTypeItem
            {
                LayerName = key,
            })
        );
        SetupImage(Design.IsDesignMode ? null : image);
    }

    public void SetupImage(ImageModelBase? image)
    {
        if (image == null)
        {
            var initialImage = new ImageModelBase
            {
                Image = new Bitmap(AssetLoader.Open(new Uri(DefaultImagePath)))
            };
            EditImageViewer.ViewImage = initialImage;
            LayerManager.InitialImage = ImageEditHelper.LoadImageSharp(initialImage.ImgPath);
        }
        else
        {
            EditImageViewer.ViewImage = image;
            LayerManager.InitialImage = ImageEditHelper.LoadImageSharp(image.ImgPath);
        }

        // Set the callback to update the image, set dirty flag
        LayerManager.UpdateImageCallback = bitmap => { EditImageViewer.ViewImage.Image = bitmap; };
        LayerManager.DirtyCallback = () => { IsDirty = true; };
        // Add initial layers
        if (LayerManager.Layers.Count == 0)
        {
            LayerManager.AddLayer(new WhiteBalanceLayerViewModel());
            LayerManager.AddLayer(new HslLayerViewModel());
            LayerManager.AddLayer(new CurveLayerViewModel());
        }

        SelectedLayer = LayerManager.Layers.LastOrDefault();
        if (SelectedLayer == null) return;
        LayerManager.RefreshFinalImage(SelectedLayer);
    }

    [RelayCommand]
    public void RemoveLayer()
    {
        if (SelectedLayer == null) return;
        var index = LayerManager.Layers.IndexOf(SelectedLayer);
        LayerManager.RemoveLayer(SelectedLayer);
        // If the removed layer was the selected layer, select the nearest layer
        if (index == LayerManager.Layers.Count) index--;
        SelectedLayer = LayerManager.Layers.ElementAtOrDefault(index);
    }


    private readonly Dictionary<string, Func<LayerBaseViewModel>> _layerConstructors = new()
    {
        { "BrightnessContrast", () => new BrightnessContrastLayerViewModel() },
        { "Sharpness", () => new SharpnessLayerViewModel() },
        { "GaussianBlur", () => new GaussianBlurLayerViewModel() },
        { "HSL", () => new HslLayerViewModel() },
        { "Curve", () => new CurveLayerViewModel() },
        { "WhiteBalance", () => new WhiteBalanceLayerViewModel() },
        { "Vignette", () => new VignetteLayerViewModel() },
        { "Grayscale", () => new GrayscaleLayerViewModel() },
        { "Pixelate", () => new PixelateLayerViewModel() }
    };

    public void AddLayer(string? layerType)
    {
        if (string.IsNullOrEmpty(layerType)) return;
        if (!_layerConstructors.TryGetValue(layerType, out var value)) return;

        var layer = value();
        LayerManager.AddLayer(layer);
        // Move the layer to above the selected layer, if no layer is selected, move to the top
        if (SelectedLayer == null) return;
        var index = LayerManager.Layers.IndexOf(SelectedLayer) + 1;
        if (index == LayerManager.Layers.Count) index--;
        LayerManager.MoveLayer(LayerManager.Layers.Last(), index);
        SelectedLayer = LayerManager.Layers.ElementAtOrDefault(index);
    }


    public ImageModelBase SaveImageToGalleryDirectory(bool saveAsNew = true)
    {
        var config = ConfigService.Instance;
        var saveDirectory = config.Settings.SaveDirectory;
        var imageModelBase = EditImageViewer.ViewImage;
        var newName = Path.GetFileNameWithoutExtension(imageModelBase.ImgName);
        if (saveAsNew)
        {
            while (Path.Exists(Path.Combine(saveDirectory, newName + ".png")))
            {
                newName += "_edited";
            }
        }

        var newFilePath = Path.Combine(saveDirectory, newName + ".png");
        imageModelBase.Image.Save(newFilePath);
        IsDirty = false;
        NotifyHelper.Notify(this, Localizer.Localizer.Instance["SaveSuccess"], $"{newFilePath}",
            type: NotificationType.Success);
        return imageModelBase;
    }
}

public partial class DesignImageEditorViewModel : ImageEditorViewModel
{
    public DesignImageEditorViewModel() : base(null)
    {
        SetupImage(null);
    }
}

public class LayerManagerViewModel : ViewModelBase
{
    private bool _isGeneratingImage;
    private CancellationTokenSource _cancellationTokenSource = new();
    private readonly bool _asyncDisplay;


    public Image<Rgba32> InitialImage { get; set; }

    public Image<Rgba32> FinalImage { get; set; }

    private Bitmap DisplayImage { get; set; }

    public ObservableCollection<LayerBaseViewModel> Layers { get; set; } = new();

    public Action<Bitmap> UpdateImageCallback { get; set; }


    public Action DirtyCallback { get; set; }


    public LayerManagerViewModel(bool asyncDisplay = true)
    {
        _asyncDisplay = asyncDisplay;
        Layers.CollectionChanged += (sender, args) => { OnPropertyChanged(nameof(Layers)); };
    }

    public void AddLayer(LayerBaseViewModel layer)
    {
        Layers.Add(layer);
        layer.LayerModified += RefreshFinalImage;
        layer.LayerModified += SetDirty;
        layer.RequestRemoveLayer += RemoveLayer;
        layer.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(LayerBaseViewModel.IsVisible)) RefreshFinalImage(layer);
        };
        if (layer.InitialUpdate) RefreshFinalImage();
    }

    public void RemoveLayer(LayerBaseViewModel layer)
    {
        layer.LayerModified -= RefreshFinalImage;
        layer.LayerModified -= SetDirty;
        layer.RequestRemoveLayer -= RemoveLayer;
        Layers.Remove(layer);
        RefreshFinalImage();
    }

    public void MoveLayer(LayerBaseViewModel layer, int newIndex)
    {
        var oldIndex = Layers.IndexOf(layer);
        if (oldIndex == -1 || newIndex < 0 || newIndex >= Layers.Count || oldIndex == newIndex) return;
        Layers.Move(oldIndex, newIndex);
        OnPropertyChanged(nameof(Layers));
        RefreshFinalImage();
    }

    public void SetDirty(LayerBaseViewModel layerBaseViewModel)
    {
        DirtyCallback.Invoke();
    }

    internal async void RefreshFinalImage(LayerBaseViewModel? modifiedLayer = null)
    {
        Image<Rgba32> finalImage;
        finalImage = Layers.Count == 0 ? InitialImage.Clone() : GenerateFinalImage();

        FinalImage = finalImage;
        DisplayImage = _asyncDisplay
            ? await Task.Run(() => ImageEditHelper.ConvertToBitmap(finalImage))
            : ImageEditHelper.ConvertToBitmap(finalImage);
        UpdateImageCallback?.Invoke(DisplayImage);
    }

    private Image<Rgba32> GenerateFinalImage()
        // private Bitmap GenerateFinalImage(LayerBaseViewModel? modifiedLayer)
    {
        if (_isGeneratingImage)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        _isGeneratingImage = true;
        var cancellationToken = _cancellationTokenSource.Token;

        try
        {
            if (Layers.Count == 0) return null;
            var finalImage = InitialImage.Clone();

            foreach (var layer in Layers)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    GC.Collect();
                    return null;
                }

                layer.InitialImage = finalImage.Clone();
                if (!layer.IsVisible) continue;
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                layer.ApplyModifiers();
#if DEBUG
                stopwatch.Stop();
                Console.WriteLine($"{layer.Name} took {stopwatch.ElapsedMilliseconds} ms");
#endif
                finalImage.Mutate(
                    x => x.DrawImage(
                        layer.ModifiedImage,
                        layer.SelectedBlendMode.Value,
                        layer.Opacity)
                );
            }

            return finalImage;
        }
        finally
        {
            _isGeneratingImage = false;
        }
    }
}