using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using VirtualStreetSnap.Services;
using VirtualStreetSnap.ViewModels;
using VirtualStreetSnap.Models;

namespace VirtualStreetSnap.Views;

public partial class SnapShotView : UserControl
{
    private Screen? _currentScreen;
    private Window? _window;

    public SnapShotView()
    {
        InitializeComponent();
        SizeChanged += UpdateRealSize;
    }

    private void GetCurrentScreenInfo()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;
        var screen = topLevel.Screens.ScreenFromVisual(topLevel);
        if (screen == null) return;
        _window = topLevel as Window;
        _currentScreen = screen;
    }

    private async void SnapshotButton_Click(object? sender, RoutedEventArgs e)
    {
        GetCurrentScreenInfo();
        if (_currentScreen == null) return;
        Overlay.IsVisible = false;
        var currentScreen = _currentScreen;
        Console.WriteLine(_window);
        var screenshot = await ScreenshotHelper.CaptureFullScreenAsync(currentScreen.Bounds);
        Overlay.IsVisible = true;

        // calculate capture area
        var appScale = currentScreen.Scaling;
        var borderCrop = 0;
        var captureArea = CaptureArea.Bounds;
        captureArea = new Rect(
            captureArea.X + _window.Position.X, captureArea.Y + _window.Position.Y,
            captureArea.Width * appScale, captureArea.Height * appScale);

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

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox comboBox || comboBox.SelectedItem is not SizeRatio SelectedSizeRatio) return;
        GetCurrentScreenInfo();
        Console.WriteLine(_window);
        if (_window == null) return;

        var contentHeight = (int)(CaptureArea.Bounds.Height);

        var width = SelectedSizeRatio.GetWidth(contentHeight);
        var borderSize = FocusBorder.BorderThickness.Left + FocusBorder.BorderThickness.Right;
        if (width < _window.MinWidth - borderSize)
        {
            width = (int)(_window.MinWidth + borderSize);
            contentHeight = SelectedSizeRatio.GetHeight(width);
        }

        var endX = _window.Position.X + (_window.Width - width) / 2;
        var endY = _window.Position.Y + _window.Height - contentHeight;

        _window.Width = width + borderSize;
        _window.Height = contentHeight + borderSize;
        _window.Position = new PixelPoint((int)endX,(int)(endY));
    }

    private void UpdateRealSize(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as CompositionGuidesViewModel;
        if (_currentScreen == null)
        {
            GetCurrentScreenInfo();
            if (_currentScreen == null) return;
        }

        var scaling = _currentScreen.Scaling;
        viewModel.RealCaptureAreaWidth = (int)(CaptureArea.Bounds.Width * scaling);
        viewModel.RealCaptureAreaHeight = (int)(CaptureArea.Bounds.Height * scaling);
    }
    // call when init
    public bool FixWindowSize()
    {   
        GetCurrentScreenInfo();
        if (_currentScreen == null || _window == null)
        {
            Console.WriteLine("No screen or window");
            return false;
        }
        var borderSize = FocusBorder.BorderThickness.Left + FocusBorder.BorderThickness.Right;
        _window.Width += borderSize;
        _window.Height += borderSize;
        return true;
    }
}