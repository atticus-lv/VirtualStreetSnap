using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Xaml.Interactivity;
using SkiaSharp;
using VirtualStreetSnap.Services;
using VirtualStreetSnap.ViewModels;

namespace VirtualStreetSnap.Behaviors;

public class ImageViewerBehavior : Behavior<Panel>
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
    
    // controls
    private Image? _viewBoxImage;
    private StackPanel? _imageInfoPanel;
    private Border? _colorPickerPanel;
    private Rectangle? _colorPickerRect;
    private TextBlock? _colorPickerTextHex;
    private TextBlock? _colorPickerTextRgb;
    private TextBlock? _colorPickerTextHsv;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject == null) return;

        AssociatedObject.Loaded += OnLoaded;
        AssociatedObject.PointerWheelChanged += Viewer_PointerWheelChanged;
        AssociatedObject.PointerPressed += Viewer_PointerPressed;
        AssociatedObject.PointerReleased += Viewer_PointerReleased;
        AssociatedObject.PointerMoved += Viewer_PointerMoved;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject == null) return;

        AssociatedObject.Loaded -= OnLoaded;
        AssociatedObject.PointerWheelChanged -= Viewer_PointerWheelChanged;
        AssociatedObject.PointerPressed -= Viewer_PointerPressed;
        AssociatedObject.PointerReleased -= Viewer_PointerReleased;
        AssociatedObject.PointerMoved -= Viewer_PointerMoved;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // Find controls
        _viewBoxImage = AssociatedObject.FindControl<Image>("ViewBoxImage");
        _imageInfoPanel = AssociatedObject.FindControl<StackPanel>("ImageInfoPanel");
        _colorPickerPanel = AssociatedObject.FindControl<Border>("ColoPickerPanel");
        _colorPickerRect = AssociatedObject.FindControl<Rectangle>("ColorPickerRect");
        _colorPickerTextHex = AssociatedObject.FindControl<TextBlock>("ColorPickerTextHex");
        _colorPickerTextRgb = AssociatedObject.FindControl<TextBlock>("ColorPickerTextRgb");
        _colorPickerTextHsv = AssociatedObject.FindControl<TextBlock>("ColorPickerTextHsv");

        // Setup transform
        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(_scaleTransform);
        transformGroup.Children.Add(_translateTransform);
        if (_viewBoxImage != null)
        {
            _viewBoxImage.RenderTransform = transformGroup;
        }
    }

    private bool IsPickingColor => AssociatedObject?.DataContext is ImageViewerViewModel { ShowColorPicker: true };

    private void Viewer_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
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

            if (_imageInfoPanel != null)
            {
                _imageInfoPanel.Opacity = _currentScale > InfoDisplayScaleThreshold ? 0 : 1;
            }
            _scaleTransform.ScaleX = _currentScale;
            _scaleTransform.ScaleY = _currentScale;

            _zoomVelocity *= ZoomAnimSpeedDecay;
            await Task.Delay(ZoomUpdateInterval);
        }

        _isZooming = false;
    }

    private void Viewer_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _lastMovePoint = e.GetPosition(AssociatedObject);

        if (!e.GetCurrentPoint(AssociatedObject).Properties.IsMiddleButtonPressed &&
            !e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed) return;

        if (IsPickingColor)
        {
            var viewModel = (ImageViewerViewModel)AssociatedObject.DataContext!;
            viewModel.ShowColorPicker = false;
            _ = PowerShellClipBoard.SetText(_colorPickerTextHex?.Text!);
            NotifyHelper.Notify(viewModel, Localizer.Localizer.Instance["Copied"], _colorPickerTextHex?.Text);
            return;
        }

        _isPanning = true;
        _lastPanPoint = _lastMovePoint;
        e.Pointer.Capture(AssociatedObject);
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
        if (AssociatedObject == null) return;
        var currentPoint = e.GetPosition(AssociatedObject);

        GetColorAndSetText(e, currentPoint);

        if (!_isPanning) return;
        var delta = e.GetPosition(AssociatedObject) - _lastPanPoint;
        _lastPanPoint = e.GetPosition(AssociatedObject);

        _translateTransform.X += delta.X;
        _translateTransform.Y += delta.Y;
    }

    private void GetColorAndSetText(PointerEventArgs e, Point currentPoint)
    {
        if (!IsPickingColor || _viewBoxImage == null) return;

        var imageBounds = _viewBoxImage.Bounds;
        var imagePoint = new Point(currentPoint.X, currentPoint.Y);

        if (imagePoint is not { X: >= 0, Y: >= 0 }) return;
        if (!(imagePoint.X < imageBounds.Width) || !(imagePoint.Y < imageBounds.Height)) return;

        var color = ScreenshotHelper.GetColorFromSKBitmap(_pickedColorImage, imagePoint);
        var colorHex = color.ToString().Substring(3);
        color.ToHsv(out var h, out var s, out var v);

        if (_colorPickerPanel != null)
        {
            Canvas.SetLeft(_colorPickerPanel, e.GetPosition(AssociatedObject).X);
            Canvas.SetTop(_colorPickerPanel, e.GetPosition(AssociatedObject).Y);
        }

        if (_colorPickerRect != null)
        {
            _colorPickerRect.Fill = new SolidColorBrush((uint)color);
        }
        if (_colorPickerTextHex != null)
        {
            _colorPickerTextHex.Text = $"#{colorHex}";
        }
        if (_colorPickerTextRgb != null)
        {
            _colorPickerTextRgb.Text = $"RGB({color.Red,3}, {color.Green,3}, {color.Blue,3})";
        }
        if (_colorPickerTextHsv != null)
        {
            _colorPickerTextHsv.Text = $"HSV({(int)h,3}, {(int)s,3}, {(int)v,3})";
        }
    }

    public void ResetImageViewBox()
    {
        _currentScale = 1.0;
        _scaleTransform.ScaleX = _currentScale;
        _scaleTransform.ScaleY = _currentScale;
        _translateTransform.X = 0;
        _translateTransform.Y = 0;
    }

    public void FlipHorizontally()
    {
        _scaleTransform.ScaleX *= -1;
    }

    public void FlipVertically()
    {
        _scaleTransform.ScaleY *= -1;
    }

    public void ShowColorPicker()
    {
        if (AssociatedObject?.DataContext is not ImageViewerViewModel viewModel) return;
        viewModel.ShowColorPicker = true;
        if (_viewBoxImage == null) return;
        _pickedColorImage = ScreenshotHelper.CaptureControlSKBitmap(_viewBoxImage);
        if (_colorPickerPanel != null)
        {
            Canvas.SetLeft(_colorPickerPanel, _lastPanPoint.X);
            Canvas.SetTop(_colorPickerPanel, _lastPanPoint.Y);
        }
    }
} 