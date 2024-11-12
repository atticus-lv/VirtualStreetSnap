using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Size = System.Drawing.Size;

namespace VirtualStreetSnap.Services;

public class ScreenshotHelper
{
    public static async Task<Bitmap> CaptureFullScreenAsync(PixelRect screenBounds)
    {
        await Task.Delay(50);
        using (var bitmap = new System.Drawing.Bitmap(screenBounds.Width, screenBounds.Height))
        {
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(screenBounds.X, screenBounds.Y, 0, 0,
                    new Size(screenBounds.Width, screenBounds.Height));
            }

            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
                return new Bitmap(ms);
            }
        }
    }

    public static Bitmap CropImage(Bitmap source, Rect cropBounds)
    {
        // Create a new RenderTargetBitmap with the specified crop bounds
        var croppedImage = new RenderTargetBitmap(new PixelSize((int)cropBounds.Width, (int)cropBounds.Height),
            new Vector(96, 96));

        // Use a drawing context to draw the cropped image
        using (var ctx = croppedImage.CreateDrawingContext(false))
        {
            ctx.DrawImage(source, cropBounds, new Rect(0, 0, cropBounds.Width, cropBounds.Height));
        }

        return croppedImage;
    }
}