using System;
using System.IO;
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
        image.Mutate(x => x.Lightness(brightness).Contrast(contrast));
    }

    public static void ApplySharpness<TPixel>(Image<TPixel> image, float sharpness)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        image.Mutate(x => x.GaussianSharpen(sharpness));
    }

    public static void ApplyGaussianBlur<TPixel>(Image<TPixel> image, float blur)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        image.Mutate(x => x.GaussianBlur(blur));
    }

    public static void ApplyGrayscale<TPixel>(Image<TPixel> image)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        image.Mutate(x => x.Grayscale());
    }

    public static void ApplyPixelate<TPixel>(Image<TPixel> image, float factor)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        if (factor == 0) return;
        var imageSize = Math.Min(image.Width, image.Height);
        int pixelSize = Math.Min((int)(factor * imageSize), 1);
        image.Mutate(x => x.Pixelate(pixelSize));
    }

    public static void ApplyVignette<TPixel>(Image<TPixel> image, float factor)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        const float minRadius = 0.5f;
        const float defaultFactor = 0.3f;
        float maxRadiusX = image.Width / 2;
        float maxRadiusY = image.Height / 2;
        var radiusX = maxRadiusX * (1 - defaultFactor) + minRadius;
        var radiusY = maxRadiusY * (1 - defaultFactor) + minRadius;
        // use darkness to control the Color of the vignette

        Rgba32 color = factor switch
        {
            >= 0 => new Rgba32(255, 255, 255, (byte)(255 * factor)),
            < 0 => new Rgba32(0, 0, 0, (byte)(255 * (1 - factor))),
            _ => default
        };

        image.Mutate(x => x.Vignette(new GraphicsOptions(), color, radiusX, radiusY,
            new Rectangle(0, 0, image.Width, image.Height)));
    }

    public static void ApplyHsl<TPixel>(Image<TPixel> image, float hue, float saturation, float lightness)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        image.Mutate(x => x.Hue(hue).Saturate(saturation).Lightness(lightness));
    }

    public static void ApplyWhiteBalance<TPixel>(Image<TPixel> image, float temperature, float tint)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        ApplyTemperature(image, temperature);
        ApplyTint(image, tint);
    }

    public static void ApplyTemperature<TPixel>(Image<TPixel> image, float temperature)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        if (temperature == 0) return;
        image.Mutate(ctx => ctx.ProcessPixelRowsAsVector4(row =>
        {
            for (int x = 0; x < row.Length; x++)
            {
                var pixel = row[x];
                var brightness = (pixel.X + pixel.Y + pixel.Z);
                pixel.X += temperature * 0.1f * brightness; // Red
                pixel.Z -= temperature * 0.1f * brightness; // Blue
                row[x] = pixel;
            }
        }));
    }

    public static void ApplyTint<TPixel>(Image<TPixel> image, float tint)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        if (tint == 0) return;
        image.Mutate(ctx => ctx.ProcessPixelRowsAsVector4(row =>
        {
            for (int x = 0; x < row.Length; x++)
            {
                var pixel = row[x];
                var brightness = (pixel.X + pixel.Y + pixel.Z) / 3.0f;
                pixel.X += tint * 0.1f * brightness;
                pixel.Z += tint * 0.1f * brightness;
                row[x] = pixel;
            }
        }));
    }


    // Convert a ImageSharp Image to Avalonia Bitmap
    public static Bitmap ConvertToBitmap<TPixel>(Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
    {
        if (image == null)
        {   
            Console.WriteLine("ConvertToBitmap, Image is null");
            return null;
        }
        
        using var ms = new MemoryStream();
        image.SaveAsBmp(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return new Bitmap(ms);
    }

    // Convert an Avalonia Bitmap to ImageSharp Image
    public static Image<Rgba32> ConvertToImageSharp(Bitmap bitmap)
    {
        if (bitmap == null)
        {
            throw new ArgumentNullException(nameof(bitmap), "Bitmap cannot be null");
        }

        using var ms = new MemoryStream();
        bitmap.Save(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return Image.Load<Rgba32>(ms);
    }
}