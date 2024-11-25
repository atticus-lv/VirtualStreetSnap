using System.Threading.Tasks;
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
    
    public ImageViewerViewModel()
    {
        ViewImage = new ImageBase();
    }

    [RelayCommand]
    public async Task CopySelectedImage()
    {
        if (ViewImage == null) return;
        await PowerShellClipBoard.SetImage(ViewImage.ImgPath);
    }
}