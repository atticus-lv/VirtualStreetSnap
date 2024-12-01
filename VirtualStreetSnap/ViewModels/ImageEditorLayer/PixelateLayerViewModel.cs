using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels.ImageEditorLayer;

public class PixelateLayerViewModel: LayerBaseViewModel
{
    public override string Name { get; set; } = "Pixelate";
    public int Factor { get; set; }

    public PixelateLayerViewModel()
    {
        Sliders.Add(new SliderViewModel(Factor)
        {
            Name = "Pixelate",
            MinValue = 0.0f,
            MaxValue = 1.0f,
            OnChange = value =>
            {
                Factor = (int)value;
                OnLayerModified();
            }
        });
    }

    public override void ApplyModifiers()
    {
        if (IsVisible && InitialImage != null)
        {
            ModifiedImage = InitialImage.Clone();
            ImageEditHelper.ApplyPixelate(ModifiedImage,Factor);
        }
    }
}