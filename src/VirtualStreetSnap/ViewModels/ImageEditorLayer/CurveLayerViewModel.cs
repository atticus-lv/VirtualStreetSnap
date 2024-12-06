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
        if (!IsVisible || InitialImage == null) return;
        ModifiedImage = InitialImage.Clone();
        // if only 2 points, then it's a straight line
        if (Curves[0].Points.Count == 2) return;
        foreach (var curveMappingViewModel in Curves)
        {
            ImageEditHelper.ApplyBezierCurve(ModifiedImage, curveMappingViewModel.MapCurve);
        }
    }
}