using System;
using System.Collections.ObjectModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace VirtualStreetSnap.ViewModels.ImageEditorLayer;

public abstract class LayerBaseViewModel : ViewModelBase
{
    public string Name { get; set; }

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

    public Image<Rgba32> InitialImage { get; set; }
    public Image<Rgba32> ModifiedImage { get; set; }
    public ObservableCollection<SliderViewModel> Sliders { get; set; } = new();

    public event Action<LayerBaseViewModel> LayerModified;

    protected void OnLayerModified()
    {
        LayerModified?.Invoke(this);
    }

    public abstract void ApplyModifiers();
}