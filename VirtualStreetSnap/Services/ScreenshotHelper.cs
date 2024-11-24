using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Skia.Helpers;
using SkiaSharp;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Point = Avalonia.Point;
using Size = System.Drawing.Size;

namespace VirtualStreetSnap.Services;

/// <summary>
/// This class provides methods to capture the screen and crop the image. Only used in the Windows version.
/// </summary>
public static class ScreenshotHelper
{
    /// <summary>
    /// Captures the full screen and returns a Bitmap.
    /// </summary>
    /// <param name="screenBounds">The bounds of the screen to capture.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the captured screen as a Bitmap.</returns>
    public static async Task<Bitmap> CaptureFullScreenAsync(PixelRect screenBounds)
    {
        await Task.Delay(5); // This is a workaround to prevent the app from freezing when capturing the screen.
        using var bitmap = new System.Drawing.Bitmap(screenBounds.Width, screenBounds.Height);
        using var g = Graphics.FromImage(bitmap);
        g.CopyFromScreen(screenBounds.X, screenBounds.Y, 0, 0,
            new Size(screenBounds.Width, screenBounds.Height));

        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Bmp);
        ms.Seek(0, SeekOrigin.Begin);
        return new Bitmap(ms);
    }
    /// <summary>
    /// Crops the specified source image to the specified bounds.
    /// </summary>
    /// <param name="source">The source image to crop.</param>
    /// <param name="cropBounds">The bounds to crop the image to.</param>
    /// <returns>The cropped image as a Bitmap.</returns>
    public static Bitmap CropImage(Bitmap source, Rect cropBounds)
    {
        var croppedImage = new RenderTargetBitmap(new PixelSize((int)cropBounds.Width, (int)cropBounds.Height),
            new Vector(96, 96));

        using var ctx = croppedImage.CreateDrawingContext(false);
        ctx.DrawImage(source, cropBounds, new Rect(0, 0, cropBounds.Width, cropBounds.Height));

        return croppedImage;
    }

    /// <summary>
    /// Captures the specified control and returns a SKBitmap.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static void CaptureControl(Visual target,string savePath)
    {
        var skBitmap = new SKBitmap((int)target.Bounds.Width, (int)target.Bounds.Height);
        using (var skCanvas = new SKCanvas(skBitmap))
        {
            DrawingContextHelper.RenderAsync(skCanvas, target);
        }

        using var image = SKImage.FromBitmap(skBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        using (var stream = System.IO.File.OpenWrite(savePath))
        {
            data.SaveTo(stream);
        }
    }
    
    /// <summary>
    /// Gets the color of the pixel at the specified point in the specified control.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public static SKColor GetColorAtControl(Visual? target, Point point)
    {
        var skBitmap = new SKBitmap((int)target.Bounds.Width, (int)target.Bounds.Height);
        using (var skCanvas = new SKCanvas(skBitmap))
        {
            DrawingContextHelper.RenderAsync(skCanvas, target);
        }

        return skBitmap.GetPixel((int)point.X, (int)point.Y);
    }
}