using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace VirtualStreetSnap.ViewModels.ImageEditorLayer;

public class LayerTypeItem
{
    public string LayerName { get; set; }
}

public class LayerBlendModeItem(
    string name = "Normal",
    PixelColorBlendingMode pixelColorBlendingMode = PixelColorBlendingMode.Normal)
{
    public string Name { get; set; } = name;
    public PixelColorBlendingMode Value { get; set; } = pixelColorBlendingMode;
}

public abstract class LayerBaseViewModel : ViewModelBase
{
    public abstract string Name { get; set; }
    public bool InitialUpdate { get; set; }

    private LayerBlendModeItem _selectedBlendMode;

    public LayerBlendModeItem SelectedBlendMode
    {
        get
        {
            _selectedBlendMode ??= LayerBlendModeItems[0];
            return _selectedBlendMode;
        }
        set
        {
            // if the selected blend mode is null or the value is null, return
            // seems this binding is after the selected layer is set, so it should not be null
            if (value == null) return;
            SetProperty(ref _selectedBlendMode, value);
            OnLayerModified();
        }
    }

    public ObservableCollection<LayerBlendModeItem> LayerBlendModeItems { get; } = new()
    {
        new("Normal", PixelColorBlendingMode.Normal),
        new("Multiply", PixelColorBlendingMode.Multiply),
        new("Add", PixelColorBlendingMode.Add),
        new("Subtract", PixelColorBlendingMode.Subtract),
        new("Screen", PixelColorBlendingMode.Screen),
        new("Overlay", PixelColorBlendingMode.Overlay),
        new("Darken", PixelColorBlendingMode.Darken),
        new("Lighten", PixelColorBlendingMode.Lighten),
        new("HardLight", PixelColorBlendingMode.HardLight),
    };

    private float _opacity = 1;

    public float Opacity
    {
        get => _opacity;
        set
        {
            // limit the opacity value to 0-1
            var opacity = Math.Clamp(value, 0, 1);
            SetProperty(ref _opacity, opacity);
            OnLayerModified();
        }
    }

    private bool _isVisible = true;

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (!SetProperty(ref _isVisible, value)) return;
            OnLayerModified();
        }
    }

    private bool _isDropTarget;

    public bool IsDropTarget
    {
        get => _isDropTarget;
        set => SetProperty(ref _isDropTarget, value);
    }

    public Image<Rgba32> InitialImage { get; set; }
    public Image<Rgba32> ModifiedImage { get; set; }
    public ObservableCollection<SliderViewModel> Sliders { get; set; } = new();

    public event Action<LayerBaseViewModel> LayerModified;
    public event Action<LayerBaseViewModel> RequestRemoveLayer;

    protected void OnLayerModified()
    {
        LayerModified?.Invoke(this);
    }

    public ICommand RemoveCommand => new RelayCommand(() => RequestRemoveLayer?.Invoke(this));


    public abstract void ApplyModifiers();
}