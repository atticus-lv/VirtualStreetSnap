using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels.ImageEditorLayer;

public class GrayscaleLayerViewModel : LayerBaseViewModel

{
    public override string Name { get; set; } = "Grayscale";

    public GrayscaleLayerViewModel()

    { }

    public override void ApplyModifiers()

    {
        if (IsVisible && InitialImage != null)

        {
            ModifiedImage = InitialImage.Clone();

            ImageEditHelper.ApplyGrayscale(ModifiedImage);
        }
    }
}