using System;
using System.ComponentModel;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.Models;

public class ImageBase
{
    private const string DefaultImagePath = "avares://VirtualStreetSnap/Assets/avalonia-logo.ico";

    public string ImgPath { get; set; } = "";
    public string ImgDir { get; set; } = "";
    public string ImgName { get; set; }
    public string ImgSize { get; set; } = "0 x 0";
    public int ImgThumbSize { get; set; } = 100;
    public Bitmap ImageThumb { get; set; }

    private Bitmap? _image { get; set; }
    
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
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    public ImageBase(string imgPath = "")
    {
        ImgPath = File.Exists(imgPath) ? imgPath : DefaultImagePath;
        ImgDir = (Path.GetDirectoryName(ImgPath) ?? "Assets/").Replace("\\", "/");
        ImgName = Path.GetFileName(ImgPath);
        LoadThumb();
    }

    private void LoadThumb()
    {
        try
        {
            ImageThumb = ImageResizer.ResizeImage(ImgPath, ImgThumbSize, ImgThumbSize);
        }
        catch (Exception)
        {
            var uri = new Uri(DefaultImagePath);
            ImageThumb = new Bitmap(AssetLoader.Open(uri));
        }
    }

    public void LoadImage()
    {
        try
        {
            Image = new Bitmap(ImgPath);
        }
        catch (Exception)
        {
            var uri = new Uri(DefaultImagePath);
            Image = new Bitmap(AssetLoader.Open(uri));
        }
    }

    public void ClearImage()
    {   // Clear the image to release the memory
        Image = null;
    }
}