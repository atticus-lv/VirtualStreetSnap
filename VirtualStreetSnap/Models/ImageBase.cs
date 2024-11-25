using System;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VirtualStreetSnap.Models;

public class ImageBase
{
    private const string DefaultImagePath = "avares://VirtualStreetSnap/Assets/avalonia-logo.ico";

    public string ImgPath { get; set; } = "";
    public string ImgDir { get; set; } = "";
    public string ImgName { get; set; }
    public string ImgSize { get; set; } = "0 x 0";
    public Bitmap Image { get; set; }

    public ImageBase(string imgPath = "")
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
            Image = new Bitmap(AssetLoader.Open(uri));
        }
    }
}