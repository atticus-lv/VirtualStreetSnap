using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using VirtualStreetSnap.Services;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace VirtualStreetSnap.ViewModels;

public class ImageLayerViewModel
{ }

public class SliderModel : ViewModelBase
{
    public string Name { get; set; }
    public float MinValue { get; set; }
    public float MaxValue { get; set; }
    public float DefaultValue { get; set; }
    public Action<float> OnChange { get; set; }

    private float _currentValue;

    public float CurrentValue
    {
        get => _currentValue;
        set
        {
            if (SetProperty(ref _currentValue, value)) OnChange?.Invoke(value);
        }
    }

    public ICommand ResetCommand { get; }

    public SliderModel(float defaultValue)
    {
        DefaultValue = defaultValue;
        _currentValue = defaultValue;
        ResetCommand = new RelayCommand(() => CurrentValue = DefaultValue);
    }
}

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
    public ObservableCollection<SliderModel> Sliders { get; set; } = new();

    public event Action<LayerBaseViewModel> LayerModified;

    protected void OnLayerModified()
    {
        LayerModified?.Invoke(this);
    }

    public abstract void ApplyModifiers();
}

public class BrightnessContrastLayerViewModel : LayerBaseViewModel
{
    public float Brightness { get; set; } = 1.0f;
    public float Contrast { get; set; } = 1.0f;

    public BrightnessContrastLayerViewModel()
    {
        Sliders.Add(new SliderModel(Brightness)
        {
            Name = "Brightness",
            MinValue = 0.0f,
            MaxValue = 2.0f,
            OnChange = value =>
            {
                Brightness = value;
                OnLayerModified();
            }
        });

        Sliders.Add(new SliderModel(Contrast)
        {
            Name = "Contrast",
            MinValue = 0.0f,
            MaxValue = 2.0f,
            OnChange = value =>
            {
                Contrast = value;
                OnLayerModified();
            }
        });
    }

    public override void ApplyModifiers()
    {
        if (IsVisible && InitialImage != null)
        {
            ModifiedImage = InitialImage.Clone();
            ImageEditHelper.ApplyBrightnessContrast(ModifiedImage, Brightness, Contrast);
        }
    }
}

public class SharpnessLayerViewModel : LayerBaseViewModel
{
    public float Sharpness { get; set; }

    public SharpnessLayerViewModel()
    {
        Sliders.Add(new SliderModel(Sharpness)
        {
            Name = "Sharpness",
            MinValue = 0.0f,
            MaxValue = 10.0f,
            OnChange = value =>
            {
                Sharpness = value;
                OnLayerModified();
            }
        });
    }

    public override void ApplyModifiers()
    {
        if (IsVisible && InitialImage != null)
        {
            ModifiedImage = InitialImage.Clone();
            ImageEditHelper.ApplySharpness(ModifiedImage, Sharpness);
        }
    }
}

public class HslLayerViewModel : LayerBaseViewModel
{
    public float Hue { get; set; }
    public float Saturation { get; set; } = 1.0f;
    public float Lightness { get; set; } = 1.0f;

    public HslLayerViewModel()
    {
        Sliders.Add(new SliderModel(Hue)
        {
            Name = "Hue",
            MinValue = -180.0f,
            MaxValue = 180.0f,
            OnChange = value =>
            {
                Hue = value;
                OnLayerModified();
            }
        });

        Sliders.Add(new SliderModel(Saturation)
        {
            Name = "Saturation",
            MinValue = 0.0f,
            MaxValue = 2.0f,
            OnChange = value =>
            {
                Saturation = value;
                OnLayerModified();
            }
        });

        Sliders.Add(new SliderModel(Lightness)
        {
            Name = "Lightness",
            MinValue = 0.0f,
            MaxValue = 2.0f,
            OnChange = value =>
            {
                Lightness = value;
                OnLayerModified();
            }
        });
    }

    public override void ApplyModifiers()
    {
        if (IsVisible && InitialImage != null)
        {
            ModifiedImage = InitialImage.Clone();
            ImageEditHelper.ApplyHsl(ModifiedImage, Hue, Saturation, Lightness);
        }
    }
}