using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace VirtualStreetSnap.ViewModels.ImageEditorLayer;

public abstract class LayerBaseViewModel : ViewModelBase
{
    public abstract string Name { get; set; }

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