using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels.ImageEditorLayer;

public class BrightnessContrastLayerViewModel : LayerBaseViewModel
{
    public float Brightness { get; set; } = 1.0f;
    public float Contrast { get; set; } = 1.0f;

    public BrightnessContrastLayerViewModel()
    {
        Sliders.Add(new SliderViewModel(Brightness)
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

        Sliders.Add(new SliderViewModel(Contrast)
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