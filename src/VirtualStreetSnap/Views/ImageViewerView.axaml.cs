using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using SkiaSharp;
using VirtualStreetSnap.Services;
using VirtualStreetSnap.ViewModels;

namespace VirtualStreetSnap.Views;

public partial class ImageViewerView : UserControl
{
    // zoom
    private const double ZoomScaleLogMap = 1.05;
    private const double InfoDisplayScaleThreshold = 1.2;
    private const double MinScale = 0.5;
    private const double MaxScale = 10.0;

    private double _currentScale = 1.0;

    // zoom animation
    private double _zoomVelocity;
    private bool _isZooming;
    private const double ZoomAnimSpeed = 1.5;
    private const double ZoomAnimStopVelocity = 0.05;
    private const double ZoomAnimSpeedDecay = 0.85;
    private const int ZoomUpdateInterval = 8; //ms

    // pan
    private Point _lastMovePoint;
    private Point _lastPanPoint;
    private bool _isPanning;
    private readonly ScaleTransform _scaleTransform = new();
    private readonly TranslateTransform _translateTransform = new();

    // color picker
    private SKBitmap? _pickedColorImage;

    public ImageViewerView()
    {
        InitializeComponent();
        
        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(_scaleTransform);
        transformGroup.Children.Add(_translateTransform);
        ViewBoxImage.RenderTransform = transformGroup;
    }

    private bool IsPickingColor => DataContext is ImageViewerViewModel { ShowColorPicker: true };

    private void Viewer_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (sender is not Control) return;
        if (IsPickingColor) return;

        var delta = e.Delta.Y > 0 ? 1 : -1;
        _zoomVelocity = delta * ZoomAnimSpeed;
        if (!_isZooming)
        {
            _isZooming = true;
            StartZoomInertia();
        }

        e.Handled = true;
    }

    private async void StartZoomInertia()
    {
        while (Math.Abs(_zoomVelocity) > ZoomAnimStopVelocity)
        {
            _currentScale *= Math.Pow(ZoomScaleLogMap, _zoomVelocity);
            _currentScale = Math.Clamp(_currentScale, MinScale, MaxScale);

            ImageInfoPanel.Opacity = _currentScale > InfoDisplayScaleThreshold ? 0 : 1;
            _scaleTransform.ScaleX = _currentScale;
            _scaleTransform.ScaleY = _currentScale;

            _zoomVelocity *= ZoomAnimSpeedDecay;
            await Task.Delay(ZoomUpdateInterval);
        }

        _isZooming = false;
    }

    private void Viewer_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _lastMovePoint = e.GetPosition(this);

        if (!e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed &&
            !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        if (IsPickingColor)
        {
            var viewModel = (ImageViewerViewModel)DataContext!;
            viewModel.ShowColorPicker = false;
            _ = PowerShellClipBoard.SetText(ColorPickerTextHex.Text!);
            return;
        }

        _isPanning = true;
        _lastPanPoint = _lastMovePoint;
        e.Pointer.Capture((IInputElement)sender!);
    }

    private void Viewer_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isPanning || (e.InitialPressMouseButton != MouseButton.Middle &&
                            e.InitialPressMouseButton != MouseButton.Left)) return;
        _isPanning = false;
        e.Pointer.Capture(null);
    }

    private void Viewer_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (sender is not Control viewbox) return;
        var currentPoint = e.GetPosition(viewbox);

        GetColorAndSetText(e, currentPoint);

        if (!_isPanning) return;
        var delta = e.GetPosition(this) - _lastPanPoint;
        _lastPanPoint = e.GetPosition(this);

        _translateTransform.X += delta.X;
        _translateTransform.Y += delta.Y;
    }

    private void GetColorAndSetText(PointerEventArgs e, Point currentPoint)
    {
        if (!IsPickingColor || ViewBoxImage is not Image image) return;

        var imageBounds = image.Bounds;
        var imagePoint = new Point(currentPoint.X, currentPoint.Y);

        if (imagePoint is not { X: >= 0, Y: >= 0 }) return;
        if (!(imagePoint.X < imageBounds.Width) || !(imagePoint.Y < imageBounds.Height)) return;

        var color = ScreenshotHelper.GetColorFromSKBitmap(_pickedColorImage, imagePoint);
        var colorHex = color.ToString().Substring(3);
        color.ToHsv(out var h, out var s, out var v);

        Canvas.SetLeft(ColoPickerPanel, e.GetPosition(this).X);
        Canvas.SetTop(ColoPickerPanel, e.GetPosition(this).Y);

        ColorPickerRect.Fill = new SolidColorBrush((uint)color);
        ColorPickerTextHex.Text = $"#{colorHex}";
        ColorPickerTextRgb.Text = $"RGB({color.Red,3}, {color.Green,3}, {color.Blue,3})";
        ColorPickerTextHsv.Text = $"HSV({(int)h,3}, {(int)s,3}, {(int)v,3})";
    }

    private void OnResetImageViewBox_Click(object? sender, RoutedEventArgs routedEventArgs)
    {
        _currentScale = 1.0;
        _scaleTransform.ScaleX = _currentScale;
        _scaleTransform.ScaleY = _currentScale;
        _translateTransform.X = 0;
        _translateTransform.Y = 0;
    }

    private void OnFlipHorizontally_Click(object? sender, RoutedEventArgs e)
    {
        _scaleTransform.ScaleX *= -1;
    }

    private void OnFlipVertically_Click(object? sender, RoutedEventArgs e)
    {
        _scaleTransform.ScaleY *= -1;
    }

    private void OnShowColorPicker_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ImageViewerViewModel viewModel) return;
        viewModel.ShowColorPicker = true;
        if (ViewBoxImage is not Image image) return;
        _pickedColorImage = ScreenshotHelper.CaptureControlSKBitmap(image);
        Canvas.SetLeft(ColoPickerPanel, _lastPanPoint.X);
        Canvas.SetTop(ColoPickerPanel, _lastPanPoint.Y);
    }
}