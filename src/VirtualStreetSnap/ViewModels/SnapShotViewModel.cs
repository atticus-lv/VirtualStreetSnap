using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels;

public partial class SnapShotViewModel : ViewModelBase
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


    private Thickness _focusBorderThickness;

    public Thickness FocusBorderThickness
    {
        get => _focusBorderThickness;
        private set => SetProperty(ref _focusBorderThickness, value);
    }

    private IImmutableSolidColorBrush _focusFocusBorderBrush;

    public IImmutableSolidColorBrush FocusBorderBrush
    {
        get => _focusFocusBorderBrush;
        private set => SetProperty(ref _focusFocusBorderBrush, value);
    }

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

    [ObservableProperty]
    private bool _isEditingSize;

    [ObservableProperty]
    private string _editWidth;

    [ObservableProperty]
    private string _editHeight;

    public SnapShotViewModel()
    {
        SelectedSizeRatio = RatioItems.First();
        ShowGuideLinesGrid = Config.Overlays.Guides.Grid;
        ShowGuideLinesCenter = Config.Overlays.Guides.Center;
        ShowGuideLinesRatio = Config.Overlays.Guides.Ratio;
        ShowFocusBorder = Config.Overlays.ShowFocusBorder;
        var thickness = Config.Overlays.FocusBorderThickness;
        FocusBorderThickness = new Thickness(thickness);
        FocusBorderBrush = new ImmutableSolidColorBrush(Config.Overlays.FocusBorderColor, 1.0);

        Config.Overlays.PropertyChanged += OnOverlaysPropertyChanged;
    }

    private void OnOverlaysPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Config.Overlays.FocusBorderColor))
            FocusBorderBrush = new ImmutableSolidColorBrush(Config.Overlays.FocusBorderColor, 1.0);
        else if (e.PropertyName == nameof(Config.Overlays.FocusBorderThickness))
            FocusBorderThickness = new Thickness(Config.Overlays.FocusBorderThickness);
    }
}