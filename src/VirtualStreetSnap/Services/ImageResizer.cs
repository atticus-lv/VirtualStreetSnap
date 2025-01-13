using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Image = SixLabors.ImageSharp.Image;

namespace VirtualStreetSnap.Services;

public static class ImageResizer
{
    public static async Task<Bitmap> ResizeImageAsync(string inputPath, int targetWidth, int targetHeight)
    {
        return await Task.Run(() => ResizeImage(inputPath, targetWidth, targetHeight));
    }

    private static Bitmap ResizeImage(string inputPath, int targetWidth, int targetHeight)
    {
        using var image = Image.Load(inputPath);
        var srcWidth = image.Width;
        var srcHeight = image.Height;

        // 如果源图像小于目标尺寸，直接返回原图
        if (srcWidth <= targetWidth && srcHeight <= targetHeight)
        {
            using var memoryStream = new MemoryStream();
            image.SaveAsPng(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return new Bitmap(memoryStream);
        }

        var aspectRatio = (float)srcWidth / srcHeight;
        int newWidth, newHeight;

        // 计算新的尺寸
        if (srcWidth > targetWidth && srcHeight > targetHeight)
        {
            newWidth = targetWidth;
            newHeight = targetHeight;
        }
        else if (srcWidth > targetWidth)
        {
            newWidth = targetWidth;
            newHeight = (int)(targetWidth / aspectRatio);
        }
        else
        {
            newHeight = targetHeight;
            newWidth = (int)(targetHeight * aspectRatio);
        }

        // 调整图像大小
        image.Mutate(x => x.Resize(newWidth, newHeight));

        // 保存到内存流
        using var memStream = new MemoryStream();
        image.SaveAsPng(memStream);
        memStream.Seek(0, SeekOrigin.Begin);
        return new Bitmap(memStream);
    }
}