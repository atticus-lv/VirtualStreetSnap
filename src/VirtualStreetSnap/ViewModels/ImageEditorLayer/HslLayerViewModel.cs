using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels.ImageEditorLayer;

public class HslLayerViewModel : LayerBaseViewModel
{
    public override string Name { get; set; } = "HSL";
    public float Hue { get; set; }
    public float Saturation { get; set; } = 1.0f;
    public float Lightness { get; set; } = 1.0f;

    public HslLayerViewModel()
    {
        Sliders.Add(new SliderViewModel(Hue)
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

        Sliders.Add(new SliderViewModel(Saturation)
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

        Sliders.Add(new SliderViewModel(Lightness)
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