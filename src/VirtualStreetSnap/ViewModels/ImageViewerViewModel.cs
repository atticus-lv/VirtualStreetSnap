using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels;

public partial class ImageViewerViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _showColorPicker;

    [ObservableProperty]
    private ImageBase? _viewImage;
    
    [ObservableProperty]
    private Bitmap? _image = null;
    
    public ImageViewerViewModel()
    {
        ViewImage = new ImageBase();
    }
}