﻿using System.IO;
using Avalonia.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace VirtualStreetSnap.Services;

public static class ImageEditHelper
{
    public static void ApplyBrightnessContrast<TPixel>(Image<TPixel> image, float brightness, float contrast)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        image.Mutate(x => x.Brightness(brightness).Contrast(contrast));
    }

    public static void ApplySharpness<TPixel>(Image<TPixel> image, float sharpness)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        image.Mutate(x => x.GaussianSharpen(sharpness));
    }

    public static void ApplyHsl<TPixel>(Image<TPixel> image, float hue, float saturation, float lightness)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        image.Mutate(x => x.Hue(hue).Saturate(saturation).Lightness(lightness));
    }

    // Convert a ImageSharp Image to Avalonia Bitmap
    public static Bitmap ConvertToBitmap<TPixel>(Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
    {
        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return new Bitmap(ms);
    }

    // Convert a Avalonia Bitmap to ImageSharp Image
    public static Image<Rgba32> ConvertToImageSharp(Bitmap bitmap)
    {
        using var ms = new MemoryStream();
        bitmap.Save(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return Image.Load<Rgba32>(ms);
    }
}