using System;
using System.IO;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VirtualStreetSnap.ViewModels;

public partial class ImageThumbViewModel : ViewModelBase
{
    private const string DefaultImagePath = "avares://VirtualStreetSnap/Assets/avalonia-logo.ico";

    [ObservableProperty]
    private string _imgPath = "";
    
    [ObservableProperty]
    public string _imgName;

    [ObservableProperty]
    public string _imgSize = "0x0";
    
    [ObservableProperty]
    private Bitmap _image;

    public ImageThumbViewModel(string imgPath)
    {
        ImgPath = File.Exists(imgPath) ? imgPath : DefaultImagePath;
        ImgName = Path.GetFileName(ImgPath);
        LoadImage(ImgPath);
        ImgSize = $"{Image.Size.Width}x{Image.Size.Height}";
    }


    private void LoadImage(string imagePath)
    {
        try
        {
            Image = new Bitmap(imagePath);
        }
        catch (Exception)
        {
            Image = new Bitmap(DefaultImagePath);
        }
    }
}