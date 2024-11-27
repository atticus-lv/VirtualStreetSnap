using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
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

    public ImageEditorViewModel(ImageBase? image)
    {
        SetupImage(Design.IsDesignMode ? null : image);
    }

    public void SetupImage(ImageBase? image)
    {
        if (image == null)
        {
            var initialImage = new ImageBase
            {
                Image = new Bitmap(AssetLoader.Open(new Uri(DefaultImagePath)))
            };
            EditImageViewer.ViewImage = initialImage;
            LayerManager.InitialImage = ImageEditHelper.ConvertToImageSharp(initialImage.Image);
        }
        else
        {
            EditImageViewer.ViewImage = image;
            LayerManager.InitialImage = ImageEditHelper.ConvertToImageSharp(image.Image);
        }

        // Set the callback to update the image
        LayerManager.UpdateImageCallback = bitmap => EditImageViewer.ViewImage.Image = bitmap;

        // Add initial layers
        LayerManager.AddLayer(new BrightnessContrastLayerViewModel { Name = "BrightnessContrast" });
        LayerManager.AddLayer(new SharpnessLayerViewModel { Name = "Sharpness" });
        LayerManager.AddLayer(new HslLayerViewModel { Name = "HSL" });
        SelectedLayer = LayerManager.Layers.LastOrDefault();
        if (SelectedLayer != null) LayerManager.RefreshFinalImage(SelectedLayer);
    }

    public void AddLayer(string? layerType)
    {
        if (string.IsNullOrEmpty(layerType)) return;
        switch (layerType)
        {
            case "BrightnessContrast":
                LayerManager.AddLayer(new BrightnessContrastLayerViewModel { Name = "BrightnessContrast" });
                break;
            case "Sharpness":
                LayerManager.AddLayer(new SharpnessLayerViewModel { Name = "Sharpness" });
                break;
            case "HSL":
                LayerManager.AddLayer(new HslLayerViewModel { Name = "HSL" });
                break;
            default:
                return;
        }

        SelectedLayer = LayerManager.Layers.Last();
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

    public LayerManagerViewModel(bool asyncDisplay = true)
    {
        _asyncDisplay = asyncDisplay;
        Layers.CollectionChanged += (sender, args) => { OnPropertyChanged(nameof(Layers)); };
    }

    public void AddLayer(LayerBaseViewModel layer)
    {
        Layers.Add(layer);
        layer.LayerModified += RefreshFinalImage;
        layer.RequestRemoveLayer += RemoveLayer;
        layer.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(LayerBaseViewModel.IsVisible)) RefreshFinalImage(layer);
        };
    }

    public void RemoveLayer(LayerBaseViewModel layer)
    {
        layer.LayerModified -= RefreshFinalImage;
        layer.RequestRemoveLayer -= RemoveLayer;
        Layers.Remove(layer);
        // release
        layer.InitialImage?.Dispose();
        layer.ModifiedImage?.Dispose();
        GC.Collect();
    }


    internal async void RefreshFinalImage(LayerBaseViewModel modifiedLayer)
    {
        var finalImage = GenerateFinalImage(modifiedLayer);
        FinalImage = finalImage;
        DisplayImage = _asyncDisplay
            ? await Task.Run(() => ImageEditHelper.ConvertToBitmap(finalImage))
            : ImageEditHelper.ConvertToBitmap(finalImage);
        UpdateImageCallback?.Invoke(DisplayImage);
    }

    private Image<Rgba32> GenerateFinalImage(LayerBaseViewModel? modifiedLayer)
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
            var startApplying = modifiedLayer == null;

            foreach (var layer in Layers)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    GC.Collect();
                    return null;
                }

                if (layer == modifiedLayer) startApplying = true;

                if (!startApplying) continue;
                if (!layer.IsVisible) continue;

                layer.InitialImage = finalImage.Clone();
                layer.ApplyModifiers();
                finalImage.Mutate(x => x.DrawImage(layer.ModifiedImage, 1));
            }

            return finalImage;
        }
        finally
        {
            _isGeneratingImage = false;
        }
    }
}