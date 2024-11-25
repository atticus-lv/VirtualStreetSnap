using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels;

public partial class CompositionGuidesViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _showFocusBorder = true;

    [ObservableProperty]
    private bool _showGuideLinesGrid = true;

    [ObservableProperty]
    private bool _showGuideLinesCenter;

    [ObservableProperty]
    private bool _showGuideLinesRatio;

    [ObservableProperty]
    private float _guideLinesOpacity = 0.5f;

    [ObservableProperty]
    private Color _borderColor = Colors.Brown;
    
    [ObservableProperty]
    private AppConfig _config = ConfigService.Instance;
    public CompositionGuidesViewModel()
    {
        ShowGuideLinesGrid = Config.Overlays.Guides.Grid;
        ShowGuideLinesCenter = Config.Overlays.Guides.Center;
        ShowGuideLinesRatio = Config.Overlays.Guides.Ratio;
        ShowFocusBorder = Config.Overlays.Focus;
    }
}