using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace VirtualStreetSnap.Services;

public static class PowerShellClipBoard
{
    public static async Task SetImage(string imagePath)
    {
        string script = $@"
            Add-Type -AssemblyName System.Windows.Forms;
            Add-Type -AssemblyName System.Drawing;
            $image = [System.Drawing.Image]::FromFile('{imagePath}');
            $imageStream = New-Object System.IO.MemoryStream;
            $image.Save($imageStream, [System.Drawing.Imaging.ImageFormat]::Png);
            $dataObj = New-Object System.Windows.Forms.DataObject('Bitmap', $image);
            $dataObj.SetData('PNG', $imageStream);
            [System.Windows.Forms.Clipboard]::SetDataObject($dataObj, $true);
        ";

        await ExecutePowerShell(script);
    }

    private static async Task ExecutePowerShell(string script)
    {
        string psFile = Path.GetTempFileName() + ".ps1";
        await File.WriteAllTextAsync(psFile, script);

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = "powershell.exe",
            Arguments =
                $"-NoProfile -NoLogo -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File \"{psFile}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();
        await process.WaitForExitAsync();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception($"PowerShell Error: {error}");
        }

        File.Delete(psFile);
    }
}
// TODO use this code to replace the PowerShellClipBoard class (Faster and more reliable)
// However, some software can not paste the image from the clipboard(for example, Wechat).
public static class ClipboardHelper
{
    [DllImport("user32.dll")]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll")]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll")]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll")]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [DllImport("user32.dll")]
    private static extern uint RegisterClipboardFormat(string lpszFormat);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll")]
    private static extern bool GlobalUnlock(IntPtr hMem);

    private const uint CF_DIB = 8;
    private const uint CF_DIBV5 = 17;
    private const uint GMEM_MOVEABLE = 0x0002;

    public static bool SetPng(string srcPng)
    {
        byte[] data;
        using (var img = new FileStream(srcPng, FileMode.Open, FileAccess.Read))
        {
            data = new byte[img.Length];
            img.Read(data, 0, data.Length);
        }

        int size = data.Length;
        IntPtr hMem = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)size);
        if (hMem == IntPtr.Zero)
        {
            Console.WriteLine("Failed to allocate global memory.");
            return false;
        }

        IntPtr lpMem = GlobalLock(hMem);
        if (lpMem == IntPtr.Zero)
        {
            Console.WriteLine("Failed to lock global memory.");
            return false;
        }

        try
        {
            Marshal.Copy(data, 0, lpMem, size);
        }
        finally
        {
            GlobalUnlock(hMem);
        }

        if (!OpenClipboard(IntPtr.Zero))
        {
            Console.WriteLine("Failed to open clipboard.");
            return false;
        }

        try
        {
            EmptyClipboard();
            uint pngFormat = RegisterClipboardFormat("PNG");
            SetClipboardData(pngFormat, hMem);
        }
        finally
        {
            CloseClipboard();
        }

        return true;
    }

    public static bool SetDib(string srcBmp)
    {
        byte[] data = File.ReadAllBytes(srcBmp);
        byte[] output = new byte[data.Length - 14];
        Array.Copy(data, 14, output, 0, output.Length);
        int size = output.Length;

        IntPtr hMem = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)size);
        if (hMem == IntPtr.Zero)
        {
            Console.WriteLine("Failed to allocate global memory.");
            return false;
        }

        IntPtr lpMem = GlobalLock(hMem);
        if (lpMem == IntPtr.Zero)
        {
            Console.WriteLine("Failed to lock global memory.");
            return false;
        }

        try
        {
            Marshal.Copy(output, 0, lpMem, size);
        }
        finally
        {
            GlobalUnlock(hMem);
        }

        uint format = output[0] switch
        {
            56 or 108 or 124 => CF_DIBV5,
            _ => CF_DIB
        };

        if (!OpenClipboard(IntPtr.Zero))
        {
            Console.WriteLine("Failed to open clipboard.");
            return false;
        }

        try
        {
            EmptyClipboard();
            SetClipboardData(format, hMem);
        }
        finally
        {
            CloseClipboard();
        }

        return true;
    }

    public static bool SetImage(string srcImg)
    {
        string extension = Path.GetExtension(srcImg).ToLower();
        return extension switch
        {
            ".bmp" => SetDib(srcImg),
            ".png" => SetPng(srcImg),
            _ => throw new NotSupportedException("Unsupported image format")
        };
    }
}