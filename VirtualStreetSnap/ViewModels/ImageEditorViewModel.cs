using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Services;

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

    public ImageEditorViewModel()
    {
        var initialImage = new ImageBase
        {
            Image = new Bitmap(AssetLoader.Open(new Uri(DefaultImagePath)))
        };
        EditImageViewer.ViewImage = initialImage;
        LayerManager.InitialImage = ImageEditHelper.ConvertToImageSharp(initialImage.Image);

        // Add initial layers
        LayerManager.AddLayer(new BrightnessContrastLayerViewModel { Name = "Brightness/Contrast" });
        LayerManager.AddLayer(new SharpnessLayerViewModel { Name = "Sharpness" });
        LayerManager.AddLayer(new HslLayerViewModel { Name = "HSL" });

        // Set the first layer as selected
        SelectedLayer = LayerManager.Layers.FirstOrDefault();
    }
}

public class LayerManagerViewModel : ViewModelBase
{
    public Image<Rgba32> InitialImage { get; set; }

    public ObservableCollection<LayerBaseViewModel> Layers { get; set; } = new();

    public void AddLayer(LayerBaseViewModel layer)
    {
        Layers.Add(layer);
        layer.LayerModified += RefreshFinalImage;
        layer.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(LayerBaseViewModel.IsVisible)) RefreshFinalImage();
        };
    }

    public void RemoveLayer(LayerBaseViewModel layer)
    {
        Layers.Remove(layer);
        layer.LayerModified -= RefreshFinalImage;
    }

    public void HideLayer(LayerBaseViewModel layer)
    {
        layer.IsVisible = false;
    }

    public void ShowLayer(LayerBaseViewModel layer)
    {
        layer.IsVisible = true;
    }

    public Bitmap DisplayImage
    {
        get
        {
            var finalImage = GenerateFinalImage();
            return finalImage != null ? ImageEditHelper.ConvertToBitmap(finalImage) : null;
        }
    }

    private void RefreshFinalImage()
    {
        foreach (var layer in Layers)
        {
            layer.ApplyModifiers();
        }
        OnPropertyChanged(nameof(DisplayImage));
    }

    private Image<Rgba32> GenerateFinalImage()
    {
        if (Layers.Count == 0 || InitialImage == null) return null;
        var finalImage = InitialImage.Clone();
        foreach (var layer in Layers)
        {
            layer.InitialImage = finalImage.Clone();
            layer.ApplyModifiers();
            if (layer.IsVisible) finalImage.Mutate(x => x.DrawImage(layer.ModifiedImage, 1));
        }

        return finalImage;
    }
}