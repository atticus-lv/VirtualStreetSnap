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

    public string ImgName;
    
    [ObservableProperty]
    private Bitmap _image;

    public ImageThumbViewModel(string imgPath)
    {
        ImgPath = File.Exists(imgPath) ? imgPath : DefaultImagePath;
        ImgName = Path.GetFileName(ImgPath);
        LoadImage(ImgPath);
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