using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.Models;

public class ImageModelBase : INotifyPropertyChanged
{
    private const string DefaultImagePath = "avares://VirtualStreetSnap/Assets/avalonia-logo.ico";
    private const string LoadingImagePath = "avares://VirtualStreetSnap/Assets/Images/LoadingImage.png";

    public string ImgPath { get; set; } = "";
    public string ImgDir { get; set; } = "";
    public string ImgName { get; set; }
    public string ImgSize { get; set; } = "0 x 0";
    public int ImgThumbSize { get; set; } = 100;

    private Bitmap? _image;
    private Bitmap? _imageThumb;

    public Bitmap Image
    {
        get => _image;
        set
        {
            _image = value;
            ImgSize = $"{value.Size.Width} x {value.Size.Height}";
            OnPropertyChanged(nameof(Image));
        }
    }

    public Bitmap? ImageThumb
    {
        get => _imageThumb;
        private set
        {
            _imageThumb = value;
            OnPropertyChanged(nameof(ImageThumb));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public ImageModelBase(string imgPath = "")
    {
        ImgPath = File.Exists(imgPath) ? imgPath : DefaultImagePath;
        ImgDir = (Path.GetDirectoryName(ImgPath) ?? "Assets/").Replace("\\", "/");
        ImgName = Path.GetFileName(ImgPath);
        ImageThumb = new Bitmap(AssetLoader.Open(new Uri(LoadingImagePath))); // Set default image initially
        LoadThumbAsync();
    }

    public async void LoadThumbAsync()
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
            await Task.Run(() => Image = new Bitmap(ImgPath));
        }
        catch (Exception)
        {
            var uri = new Uri(DefaultImagePath);
            Image = new Bitmap(AssetLoader.Open(uri));
        }
    }
}