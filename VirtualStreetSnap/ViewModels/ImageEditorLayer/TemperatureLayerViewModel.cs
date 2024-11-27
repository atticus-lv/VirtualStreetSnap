using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels.ImageEditorLayer;

public class TemperatureLayerViewModel: LayerBaseViewModel
{   
    public override string Name { get; set; } = "Temperature";
    public float Temperature { get; set; }

    public TemperatureLayerViewModel()
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
    }

    public override void ApplyModifiers()
    {
        if (IsVisible && InitialImage != null)
        {
            ModifiedImage = InitialImage.Clone();
            ImageEditHelper.ApplyTemperature(ModifiedImage, Temperature);
        }
    }
}