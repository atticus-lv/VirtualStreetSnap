using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using VirtualStreetSnap.Services;
using VirtualStreetSnap.ViewModels;

namespace VirtualStreetSnap.Models;

public partial class ImageModelBase : ViewModelBase
{
    private const string DefaultImagePath = "avares://VirtualStreetSnap/Assets/avalonia-logo.ico";
    private const string LoadingImagePath = "avares://VirtualStreetSnap/Assets/Images/LoadingImage.png";

    [ObservableProperty]
    private string _imgPath = "";

    [ObservableProperty]
    private string _imgDir = "";

    [ObservableProperty]
    private string _imgName;

    [ObservableProperty]
    private string _imgSize = "0 x 0";

    [ObservableProperty]
    private Bitmap? _imageThumb;

    [ObservableProperty]
    private Bitmap? _image;

    public int ImgThumbSize { get; set; } = 100;

    public ImageModelBase(string imgPath = "")
    {
        ImgPath = File.Exists(imgPath) ? imgPath : DefaultImagePath;
        ImgDir = (Path.GetDirectoryName(ImgPath) ?? "Assets/").Replace("\\", "/");
        ImgName = Path.GetFileName(ImgPath);
        ImageThumb = new Bitmap(AssetLoader.Open(new Uri(LoadingImagePath))); // Set default image initially
        LoadThumbAsync();
    }


    partial void OnImageChanged(Bitmap value)
    {
        ImgSize = $"{value.Size.Width} x {value.Size.Height}";
    }


    public async Task LoadThumbAsync()
    {
        try
        {
            ImageThumb = await ImageResizer.ResizeImageAsync(ImgPath, ImgThumbSize, ImgThumbSize);
        }
        catch (Exception)
        {
            var uri = new Uri(DefaultImagePath);
            ImageThumb = new Bitmap(AssetLoader.Open(uri));
        }
    }

    public async Task LoadImageAsync()
    {
        try
        {
            var loadImageTask = Task.Run(() => Image = new Bitmap(ImgPath));
            var loadThumbTask = LoadThumbAsync();
            await Task.WhenAll(loadImageTask, loadThumbTask);
        }
        catch (Exception)
        {
            var uri = new Uri(DefaultImagePath);
            Image = new Bitmap(AssetLoader.Open(uri));
        }
    }
}