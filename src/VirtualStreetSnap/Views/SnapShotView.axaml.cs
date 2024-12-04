using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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

        var _viewModel = DataContext as SnapShotViewModel;
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
        var remainWidth = _window.Width - CaptureArea.Bounds.Width - borderSize;
        _window.Width = width + borderSize + remainWidth;
        _window.Height = contentHeight + borderSize;
        _window.Position = new PixelPoint((int)endX, (int)(endY));
    }

    private void UpdateRealSize(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as SnapShotViewModel;
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
        // the grab bar on the right/left side
        var remainWidth = _window.Width - CaptureArea.Bounds.Width - borderSize;
        _window.Width += borderSize + remainWidth;
        _window.Height += borderSize;
        return true;
    }

    private void UpdateWindowSize(int newWidth, int newHeight)
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window == null) return;
        var borderSize = FocusBorder.BorderThickness.Left + FocusBorder.BorderThickness.Right;
        var remainWidth = _window.Width - CaptureArea.Bounds.Width - borderSize;
        window.Width = newWidth + borderSize + remainWidth;
        window.Height = newHeight + borderSize;
    }

    private void EditTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as SnapShotViewModel;
        if (viewModel == null) return;
        viewModel.IsEditingSize = false;
    }

    private void EditTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        GetCurrentScreenInfo();
        var viewModel = DataContext as SnapShotViewModel;
        if (viewModel == null || e.Key != Key.Enter) return;
        if (int.TryParse(viewModel.EditWidth, out var newWidth) &&
            int.TryParse(viewModel.EditHeight, out var newHeight))
        {
            var scaling = _currentScreen?.Scaling ?? 1;
            var realWidth = (int)(newWidth / scaling);
            var realHeight = (int)(newHeight / scaling);
            UpdateWindowSize(realWidth, realHeight);
        }
        else
        {
            viewModel.EditHeight = viewModel.RealCaptureAreaHeight.ToString();
            viewModel.EditWidth = viewModel.RealCaptureAreaWidth.ToString();
        }

        viewModel.IsEditingSize = false;
    }

    private void DisplayTextBlock_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var viewModel = DataContext as SnapShotViewModel;
        if (viewModel == null) return;
        viewModel.EditWidth = viewModel.RealCaptureAreaWidth.ToString();
        viewModel.EditHeight = viewModel.RealCaptureAreaHeight.ToString();
        viewModel.IsEditingSize = true;
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {   
        var _window = TopLevel.GetTopLevel(this) as Window;
        _window?.BeginMoveDrag(e);
    }
}