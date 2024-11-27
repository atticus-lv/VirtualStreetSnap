using System;
using System.Collections.ObjectModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels;

public class ImageLayerViewModel
{ }

public class SliderModel : ViewModelBase
{
    public string Name { get; set; }
    public float Value { get; set; }
    public float MinValue { get; set; }
    public float MaxValue { get; set; }
    public Action<float> OnChange { get; set; }

    private float _currentValue;

    public float CurrentValue
    {
        get => _currentValue;
        set
        {
            if (SetProperty(ref _currentValue, value))
            {
                OnChange?.Invoke(value);
            }
        }
    }
}

public abstract class LayerBaseViewModel : ViewModelBase
{
    public string Name { get; set; }
    public bool IsVisible { get; set; } = true;
    public Image<Rgba32> InitialImage { get; set; }
    public Image<Rgba32> ModifiedImage { get; set; }
    public ObservableCollection<SliderModel> Sliders { get; set; } = new ObservableCollection<SliderModel>();

    public event Action LayerModified;

    protected void OnLayerModified()
    {
        LayerModified?.Invoke();
    }

    public abstract void ApplyModifiers();
}

public class BrightnessContrastLayerViewModel : LayerBaseViewModel
{
    public float Brightness { get; set; } = 1.0f;
    public float Contrast { get; set; } = 1.0f;

    public BrightnessContrastLayerViewModel()
    {
        Sliders.Add(new SliderModel
        {
            Name = "Brightness",
            MinValue = 0.0f,
            MaxValue = 2.0f,
            Value = Brightness,
            OnChange = value =>
            {
                Brightness = value;
                OnLayerModified();
            }
        });

        Sliders.Add(new SliderModel
        {
            Name = "Contrast",
            MinValue = 0.0f,
            MaxValue = 2.0f,
            Value = Contrast,
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
    public float Sharpness { get; set; } = 0.0f;

    public SharpnessLayerViewModel()
    {
        Sliders.Add(new SliderModel
        {
            Name = "Sharpness",
            MinValue = 0.0f,
            MaxValue = 10.0f,
            Value = Sharpness,
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
    public float Hue { get; set; } = 0.0f;
    public float Saturation { get; set; } = 1.0f;
    public float Lightness { get; set; } = 1.0f;

    public HslLayerViewModel()
    {
        Sliders.Add(new SliderModel
        {
            Name = "Hue",
            MinValue = -180.0f,
            MaxValue = 180.0f,
            Value = Hue,
            OnChange = value =>
            {
                Hue = value;
                OnLayerModified();
            }
        });

        Sliders.Add(new SliderModel
        {
            Name = "Saturation",
            MinValue = 0.0f,
            MaxValue = 2.0f,
            Value = Saturation,
            OnChange = value =>
            {
                Saturation = value;
                OnLayerModified();
            }
        });

        Sliders.Add(new SliderModel
        {
            Name = "Lightness",
            MinValue = 0.0f,
            MaxValue = 2.0f,
            Value = Lightness,
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