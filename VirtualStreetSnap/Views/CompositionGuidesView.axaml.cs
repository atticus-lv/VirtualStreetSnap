using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using VirtualStreetSnap.Services;
using VirtualStreetSnap.ViewModels;

namespace VirtualStreetSnap.Views;

public partial class CompositionGuidesView : UserControl
{
    private Screen? _currentScreen;
    private Window? _window;

    public CompositionGuidesView()
    {
        InitializeComponent();
    }

    private void GetCurrentScreenInfo()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        Console.WriteLine("TopLevel: " + topLevel);
        if (topLevel == null) return;
        var screen = topLevel.Screens.ScreenFromVisual(topLevel);
        Console.WriteLine("Screen: " + screen);
        if (screen == null) return;
        _window = topLevel as Window;
        _currentScreen = screen;
    }

    private async void SnapshotButton_Click(object? sender, RoutedEventArgs e)
    {
        GetCurrentScreenInfo();
        if (_currentScreen == null) return;
        CaptureArea.IsVisible = false;
        var currentScreen = _currentScreen;
        var screenshot = await ScreenshotHelper.CaptureFullScreenAsync(currentScreen.Bounds);
        CaptureArea.IsVisible = true;

        // calculate capture area
        var appScale = currentScreen.Scaling;
        var borderCrop = 2 * appScale;
        var captureArea = CaptureArea.Bounds;
        captureArea = new Rect(captureArea.X + _window.Position.X + borderCrop,
            captureArea.Y + _window.Position.Y + borderCrop,
            captureArea.Width * appScale - borderCrop, captureArea.Height * appScale - borderCrop);

        var captureShot = ScreenshotHelper.CropImage(screenshot, captureArea);
        // save screenshot

        var _viewModel = DataContext as CompositionGuidesViewModel;
        var saveDir = _viewModel.Config.Settings.SaveDirectory;
        if (!Path.Exists(saveDir)) Directory.CreateDirectory(saveDir);
        var filePrefix = _viewModel.Config.Settings.FilePrefix;
        filePrefix = new string(filePrefix.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());

        var time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        // screenshot.Save(Path.Combine(path, $"FULL_{time}.png"));
        captureShot.Save(Path.Combine(saveDir, $"{filePrefix}_{time}.png"));
    }
}