using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels.ImageEditorLayer;

public class SharpnessLayerViewModel : LayerBaseViewModel
{
    public float Sharpness { get; set; }

    public SharpnessLayerViewModel()
    {
        Sliders.Add(new SliderViewModel(Sharpness)
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