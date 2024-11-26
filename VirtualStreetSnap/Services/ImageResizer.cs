using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Avalonia.Media.Imaging;
using Bitmap = Avalonia.Media.Imaging.Bitmap;


namespace VirtualStreetSnap.Services;

public static class ImageResizer
{
    public static Bitmap ResizeImage(string inputPath, int targetWidth, int targetHeight)
    {
        using var srcImage = Image.FromFile(inputPath);
        var srcWidth = srcImage.Width;
        var srcHeight = srcImage.Height;
        // If the size of the source image is smaller than the target size, the original image is returned  
        if (srcWidth <= targetWidth && srcHeight <= targetHeight)
        {
            using var memoryStream = new MemoryStream();
            srcImage.Save(memoryStream, ImageFormat.Png);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return new Bitmap(memoryStream);
        }

        var aspectRatio = (float)srcWidth / srcHeight;
        int newWidth, newHeight;
        // If the source image size is larger than the target size, crop to the target size  
        if (srcWidth > targetWidth && srcHeight > targetHeight)
        {
            newWidth = targetWidth;
            newHeight = targetHeight;
        }
        // If the source image width is larger than the target width, scale by width
        else if (srcWidth > targetWidth)
        {
            newWidth = targetWidth;
            newHeight = (int)(targetWidth / aspectRatio);
        }
        // If the source image height is greater than the target height, scale by height      
        else
        {
            newHeight = targetHeight;
            newWidth = (int)(targetHeight * aspectRatio);
        }

        var destRect = new Rectangle(0, 0, newWidth, newHeight);
        var destImage = new System.Drawing.Bitmap(newWidth, newHeight);
        destImage.SetResolution(srcImage.HorizontalResolution, srcImage.VerticalResolution);
        using (var graphics = Graphics.FromImage(destImage))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            using (var wrapMode = new ImageAttributes())
            {
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(srcImage, destRect, 0, 0, srcImage.Width, srcImage.Height, GraphicsUnit.Pixel,
                    wrapMode);
            }
        }

        using (var memoryStream = new MemoryStream())
        {
            destImage.Save(memoryStream, ImageFormat.Png);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return new Bitmap(memoryStream);
        }
    }
}