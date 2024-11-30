using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
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
    private string _dragItemText;

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
        if (LayerManager.Layers.Count == 0)
        {
            LayerManager.AddLayer(new TemperatureLayerViewModel());
            LayerManager.AddLayer(new BrightnessContrastLayerViewModel());
            LayerManager.AddLayer(new HslLayerViewModel());
        }

        SelectedLayer = LayerManager.Layers.LastOrDefault();
        if (SelectedLayer != null) LayerManager.RefreshFinalImage(SelectedLayer);
    }

    [RelayCommand]
    public void RemoveLayer()
    {
        var index = LayerManager.Layers.IndexOf(SelectedLayer);
        LayerManager.RemoveLayer(SelectedLayer);
        // If the removed layer was the selected layer, select the nearest layer
        if (index == LayerManager.Layers.Count) index--;
        SelectedLayer = LayerManager.Layers.ElementAtOrDefault(index);
    }

    [RelayCommand]
    public void MoveLayerUp()
    {
        var index = LayerManager.Layers.IndexOf(SelectedLayer);
        LayerManager.MoveLayer(SelectedLayer, index + 1);
    }

    [RelayCommand]
    public void MoveLayerDown(LayerBaseViewModel layer)
    {
        var index = LayerManager.Layers.IndexOf(SelectedLayer);
        LayerManager.MoveLayer(SelectedLayer, index - 1);
    }

    public void AddLayer(string? layerType)
    {
        if (string.IsNullOrEmpty(layerType)) return;
        switch (layerType)
        {
            case "BrightnessContrast":
                LayerManager.AddLayer(new BrightnessContrastLayerViewModel());
                break;
            case "Sharpness":
                LayerManager.AddLayer(new SharpnessLayerViewModel());
                break;
            case "HSL":
                LayerManager.AddLayer(new HslLayerViewModel());
                break;
            case "Temperature":
                LayerManager.AddLayer(new TemperatureLayerViewModel());
                break;
            case "Tint":
                LayerManager.AddLayer(new TintLayerViewModel());
                break;
            default:
                return;
        }

        // move the layer to the top of selected layer, if no layer is selected, move to the top
        var index = SelectedLayer == null ? 0 : LayerManager.Layers.IndexOf(SelectedLayer);
        LayerManager.MoveLayer(LayerManager.Layers.Last(), index);
    }
    
    [RelayCommand]
    public void SaveImage()
    {
        var imageBase = SaveImageToGalleryDirectory(saveAsNew: true);
    }
    
    public ImageBase SaveImageToGalleryDirectory(bool saveAsNew = true)
    {
        var imageBase = EditImageViewer.ViewImage;
        if (!saveAsNew) return imageBase;
        var newName = imageBase.ImgName + "_edited";
        var config = ConfigService.Instance;
        var saveDirectory = config.Settings.SaveDirectory;
        var newFilePath = Path.Combine(saveDirectory, newName + ".png");
        imageBase.Image.Save(newFilePath);
        imageBase.LoadThumbAsync();
        return imageBase;
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
        GenerateFinalImage();
    }

    public void RemoveLayer(LayerBaseViewModel layer)
    {
        layer.LayerModified -= RefreshFinalImage;
        layer.RequestRemoveLayer -= RemoveLayer;
        Layers.Remove(layer);
        layer.InitialImage?.Dispose();
        layer.ModifiedImage?.Dispose();
        GC.Collect();
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

    internal async void RefreshFinalImage(LayerBaseViewModel? modifiedLayer = null)
    {
        var finalImage = GenerateFinalImage();
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