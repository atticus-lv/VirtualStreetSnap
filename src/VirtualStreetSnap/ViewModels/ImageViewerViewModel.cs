using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using VirtualStreetSnap.Models;

namespace VirtualStreetSnap.ViewModels;

public partial class ImageViewerViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _showColorPicker;

    [ObservableProperty]
    private ImageBase? _viewImage;
    
    [ObservableProperty]
    private Bitmap? _image;
    
    public ImageViewerViewModel()
    {
        ViewImage = new ImageBase();
    }
}