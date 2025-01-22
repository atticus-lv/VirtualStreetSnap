using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Skia.Helpers;
using SkiaSharp;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Point = Avalonia.Point;
using System.Drawing;
using System.Drawing.Imaging;
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
    public static async Task<Bitmap> CaptureFullScreenAsync(PixelRect screenBounds, int msDelay = 10)
    {
        await Task.Delay(msDelay);

#if OSX
        // macOS 实现
        var tempPath = await CaptureScreenMacOs();
        if (string.IsNullOrEmpty(tempPath))
        {
            throw new Exception("截图失败");
        }

        try
        {
            using var fileStream = File.OpenRead(tempPath);
            var bitmap = new Bitmap(fileStream);
            File.Delete(tempPath); // 删除临时文件
            return bitmap;
        }
        catch (Exception ex)
        {
            File.Delete(tempPath); // 确保清理临时文件
            throw new Exception($"处理截图失败: {ex.Message}");
        }
#elif LINUX
        // Linux 实现
        var tempPath = await CaptureScreenLinux();
        if (string.IsNullOrEmpty(tempPath))
        {
            throw new Exception("截图失败");
        }

        try
        {
            using var fileStream = File.OpenRead(tempPath);
            var bitmap = new Bitmap(fileStream);
            File.Delete(tempPath); // 删除临时文件
            return bitmap;
        }
        catch (Exception ex)
        {
            File.Delete(tempPath); // 确保清理临时文件
            throw new Exception($"处理截图失败: {ex.Message}");
        }
#else
        // Windows 实现（默认）
        using var bitmap = new System.Drawing.Bitmap(screenBounds.Width, screenBounds.Height);
        using var g = Graphics.FromImage(bitmap);
        g.CopyFromScreen(screenBounds.X, screenBounds.Y, 0, 0,
            new Size(screenBounds.Width, screenBounds.Height));

        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Bmp);
        ms.Seek(0, SeekOrigin.Begin);
        return new Bitmap(ms);
#endif
    }

#if OSX
    private static async Task<string> CaptureScreenMacOs()
    {
        var fileName = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        var tempPath = Path.Combine(Path.GetTempPath(), fileName);

        var startInfo = new ProcessStartInfo
        {
            FileName = "screencapture",
            Arguments = $"-x \"{tempPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        await process!.WaitForExitAsync();

        return process.ExitCode == 0 ? tempPath : string.Empty;
    }
#endif

#if LINUX
    private static async Task<string> CaptureScreenLinux()
    {
        var fileName = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        var tempPath = Path.Combine(Path.GetTempPath(), fileName);

        // 尝试使用不同的截图工具
        var screenshotTools = new[]
        {
            ("scrot", $"\"{tempPath}\""),  // 通用
            ("gnome-screenshot", $"--file=\"{tempPath}\""),  // GNOME
            ("spectacle", $"-b -n -o \"{tempPath}\"")  // KDE
        };

        foreach (var (tool, args) in screenshotTools)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = tool,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null) continue;

                await process.WaitForExitAsync();

                // 检查文件是否成功创建
                if (process.ExitCode == 0 && File.Exists(tempPath))
                {
                    return tempPath;
                }
            }
            catch (Exception)
            {
                // 如果当前工具不可用，继续尝试下一个
                continue;
            }
        }

        return string.Empty;  // 所有工具都失败时返回空
    }
#endif
    
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


    // Method to capture the control as an SKBitmap
    public static SKBitmap CaptureControlSKBitmap(Visual? target)
    {
        var skBitmap = new SKBitmap((int)target.Bounds.Width, (int)target.Bounds.Height);
        using var skCanvas = new SKCanvas(skBitmap);
        DrawingContextHelper.RenderAsync(skCanvas, target);

        return skBitmap;
    }

    // Method to get the color at a specific point from an SKBitmap
    public static SKColor GetColorFromSKBitmap(SKBitmap bitmap, Point point)
    {
        return bitmap.GetPixel((int)point.X, (int)point.Y);
    }

    public static void SaveSkBitmap(SKBitmap bitmap, string path)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(path);
        data.SaveTo(stream);
    }
}