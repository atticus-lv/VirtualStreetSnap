using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels.ImageEditorLayer;

public class TintLayerViewModel: LayerBaseViewModel
{   
    public override string Name { get; set; } = "Tint";
    public float Tint { get; set; }

    public TintLayerViewModel()
    {
        Sliders.Add(new SliderViewModel(Tint)
        {
            Name = "Tint",
            MinValue = -1f,
            MaxValue = 1f,
            OnChange = value =>
            {
                Tint = value;
                OnLayerModified();
            }
        });
    }

    public override void ApplyModifiers()
    {
        if (IsVisible && InitialImage != null)
        {
            ModifiedImage = InitialImage.Clone();
            ImageEditHelper.ApplyTint(ModifiedImage, Tint);
        }
    }
}