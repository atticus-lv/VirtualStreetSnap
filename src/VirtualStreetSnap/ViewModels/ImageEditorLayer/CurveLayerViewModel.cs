using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels.ImageEditorLayer;

public class CurveLayerViewModel : LayerBaseViewModel
{
    public override string Name { get; set; } = "Curve";

    public CurveLayerViewModel()
    {
        Curves.Add(new CurveMappingViewModel()
        {
            OnChange = () => { OnLayerModified(); }
        });
    }

    public override void ApplyModifiers()
    {
        if (IsVisible && InitialImage != null)
        {
            ModifiedImage = InitialImage.Clone();
            foreach (var curveMappingViewModel in Curves)
            {   
                ImageEditHelper.ApplyBezierCurve(ModifiedImage, curveMappingViewModel.MapCurve);
            }
        }
    }
}