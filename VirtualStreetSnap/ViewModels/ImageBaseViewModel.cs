using System;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VirtualStreetSnap.ViewModels;

public partial class ImageBaseViewModel : ViewModelBase
{
    private const string DefaultImagePath = "avares://VirtualStreetSnap/Assets/avalonia-logo.ico";

    [ObservableProperty]
    private string _imgPath = "";

    [ObservableProperty]
    private string _imgDir = "";

    [ObservableProperty]
    public string _imgName;

    [ObservableProperty]
    public string _imgSize = "0 x 0";

    [ObservableProperty]
    private Bitmap _image;

    public ImageBaseViewModel(string imgPath = "")
    {
        ImgPath = File.Exists(imgPath) ? imgPath : DefaultImagePath;
        ImgDir = (Path.GetDirectoryName(ImgPath) ?? "Assets/").Replace("\\", "/");
        ImgName = Path.GetFileName(ImgPath);
        LoadImage(ImgPath);
        ImgSize = $"{Image.Size.Width} x {Image.Size.Height}";
    }


    private void LoadImage(string imagePath)
    {
        try
        {
            Image = new Bitmap(imagePath);
        }
        catch (Exception)
        {
            var uri = new Uri(DefaultImagePath);
            Image =  new Bitmap(AssetLoader.Open(uri));
        }
    }
}