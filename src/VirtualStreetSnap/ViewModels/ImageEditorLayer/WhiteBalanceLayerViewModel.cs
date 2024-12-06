using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels.ImageEditorLayer;

public class WhiteBalanceLayerViewModel : LayerBaseViewModel
{
    public override string Name { get; set; } = "WhiteBalance";
    public float Temperature { get; set; }
    public float Tint { get; set; }

    public WhiteBalanceLayerViewModel()
    {
        Sliders.Add(new SliderViewModel(Temperature)
        {
            Name = "Temperature",
            MinValue = -1f,
            MaxValue = 1f,
            OnChange = value =>
            {
                Temperature = value;
                OnLayerModified();
            }
        });

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
            ImageEditHelper.ApplyWhiteBalance(ModifiedImage, Temperature, Tint);
        }
    }
}