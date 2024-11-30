using System.Collections.ObjectModel;
using System.Linq;
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
    private SizeRatio? _selectedSizeRatio;

    public ObservableCollection<SizeRatio> RatioItems { get; } =
    [
        new("16:9"),
        new("4:3"),
        new("3:2"),
        new("1:1"),
        new("3:4"),
        new("9:16")
    ];


    [ObservableProperty]
    private int _realCaptureAreaWidth;

    [ObservableProperty]
    private int _realCaptureAreaHeight;

    [ObservableProperty]
    private AppConfig _config = ConfigService.Instance;

    public CompositionGuidesViewModel()
    {
        SelectedSizeRatio = RatioItems.First();
        ShowGuideLinesGrid = Config.Overlays.Guides.Grid;
        ShowGuideLinesCenter = Config.Overlays.Guides.Center;
        ShowGuideLinesRatio = Config.Overlays.Guides.Ratio;
        ShowFocusBorder = Config.Overlays.Focus;
    }
}