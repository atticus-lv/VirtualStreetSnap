using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels.ImageEditorLayer;

public class VignetteLayerViewModel : LayerBaseViewModel
{
    public override string Name { get; set; } = "Vignette";
    public float Factor { get; set; }

    public VignetteLayerViewModel()
    {
        Sliders.Add(new SliderViewModel(Factor)
        {
            Name = "Factor",
            MinValue = -1.0f,
            MaxValue = 1.0f,
            OnChange = value =>
            {
                Factor = value;
                OnLayerModified();
            }
        });
    }

    public override void ApplyModifiers()
    {
        if (IsVisible && InitialImage != null)
        {
            ModifiedImage = InitialImage.Clone();
            ImageEditHelper.ApplyVignette(ModifiedImage, Factor);
        }
    }
}