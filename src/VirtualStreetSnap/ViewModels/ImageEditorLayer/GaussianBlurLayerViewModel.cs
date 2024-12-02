using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels.ImageEditorLayer;

public class GaussianBlurLayerViewModel : LayerBaseViewModel
{
    public override string Name { get; set; } = "Gaussian Blur";
    public float Radius { get; set; }

    public GaussianBlurLayerViewModel()
    {
        Sliders.Add(new SliderViewModel(Radius)
        {
            Name = "Factor",
            MinValue = 0.0f,
            MaxValue = 10.0f,
            OnChange = value =>
            {
                Radius = value;
                OnLayerModified();
            }
        });
    }

    public override void ApplyModifiers()
    {
        if (IsVisible && InitialImage != null)
        {
            ModifiedImage = InitialImage.Clone();
            ImageEditHelper.ApplyGaussianBlur(ModifiedImage, Radius);
        }
    }
}