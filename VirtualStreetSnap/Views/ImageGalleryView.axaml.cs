using System;
using System.Drawing;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using VirtualStreetSnap.ViewModels;
using Point = Avalonia.Point;
using System.Windows.Forms;
using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.Views;

public partial class ImageGalleryView : UserControl
{
    private double _currentScale = 1.0;
    private const double ScaleStep = 0.1;
    private const double MinScale = 0.5;
    private const double MaxScale = 10.0;
    private Point _lastMovePoint;
    private Point _lastPanPoint;
    private bool _isPanning;
    private readonly ScaleTransform _scaleTransform = new();
    private readonly TranslateTransform _translateTransform = new();

    public ImageGalleryView()
    {
        InitializeComponent();
        var scrollViewer = this.FindControl<ScrollViewer>("ThumbnailsScrollViewer");
        scrollViewer.PointerWheelChanged += ScrollViewer_PointerWheelChanged;

        var imageViewbox = this.FindControl<Viewbox>("ImageViewbox");
        imageViewbox.PointerWheelChanged += ImageViewbox_PointerWheelChanged;

        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(_scaleTransform);
        transformGroup.Children.Add(_translateTransform);
        imageViewbox.RenderTransform = transformGroup;
    }

    private bool IsPickingColor => DataContext is ImageGalleryViewModel { ShowColorPicker: true };


    private void ScrollViewer_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;
        const double scrollFactor = 30; // Adjust this factor to increase or decrease the scroll distance
        scrollViewer.Offset = new Vector(scrollViewer.Offset.X - e.Delta.Y * scrollFactor, scrollViewer.Offset.Y);
        e.Handled = true;
    }

    private void ThumbnailsScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;
        if (!(scrollViewer.Offset.X >= scrollViewer.Extent.Width - scrollViewer.Viewport.Width)) return;
        if (DataContext is ImageGalleryViewModel viewModel) viewModel.LoadMoreThumbnailsCommand.Execute(null);
    }


    private void ImageViewbox_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (sender is not Viewbox) return;
        if (IsPickingColor) return;

        _currentScale = e.Delta.Y > 0
            ? Math.Min(_currentScale + ScaleStep, MaxScale)
            : Math.Max(_currentScale - ScaleStep, MinScale);

        _scaleTransform.ScaleX = _currentScale;
        _scaleTransform.ScaleY = _currentScale;
        e.Handled = true;
    }

    private void ImageViewbox_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _lastMovePoint = e.GetPosition(this);

        if (!e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed &&
            !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        // Hide color picker when left mouse button is pressed, not middle button

        if (IsPickingColor)
        {
            var viewModel = (ImageGalleryViewModel)DataContext!;
            viewModel.ShowColorPicker = false;
            _ = PowerShellClipBoard.SetText(ColorPickerTextHex.Text!);
            return; // Exit early
        }

        _isPanning = true;
        _lastPanPoint = _lastMovePoint;
        e.Pointer.Capture((IInputElement)sender!);
    }

    private void ImageViewbox_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isPanning || (e.InitialPressMouseButton != MouseButton.Middle &&
                            e.InitialPressMouseButton != MouseButton.Left)) return;
        _isPanning = false;
        e.Pointer.Capture(null);
    }

    private void ImageViewbox_PointerMoved(object? sender, PointerEventArgs e)
    {
        var position = e.GetPosition(this);
        var color = ScreenshotHelper.GetColorAtControl((Visual)sender!, position);
        var colorHex = color.ToString().Substring(3);
        Canvas.SetLeft(ColoPickerPanel, position.X);
        Canvas.SetTop(ColoPickerPanel, position.Y);
        Console.WriteLine(Canvas.GetLeft(ColoPickerPanel));
        ColorPickerRect.Fill = new SolidColorBrush((uint)color);
        ColorPickerTextHex.Text = $"#{colorHex}";
        ColorPickerTextRgb.Text = $" RGB({color.Red}, {color.Green}, {color.Blue})";


        if (!_isPanning) return;
        var currentPoint = e.GetPosition(this);
        var delta = currentPoint - _lastPanPoint;
        _lastPanPoint = currentPoint;

        _translateTransform.X += delta.X;
        _translateTransform.Y += delta.Y;
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
        if (DataContext is ImageGalleryViewModel viewModel)
        {
            viewModel.ShowColorPicker = true;
        }
    }
}