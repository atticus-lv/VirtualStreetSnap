using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Markup.Xaml;
using VirtualStreetSnap.ViewModels;

namespace VirtualStreetSnap.Views;

public partial class ImageGalleryView : UserControl
{
    private double _currentScale = 1.0;
    private const double ScaleStep = 0.1;
    private const double MinScale = 0.5;
    private const double MaxScale = 10.0;
    private Point _lastPanPoint;
    private bool _isPanning;
    private readonly ScaleTransform _scaleTransform = new();
    private readonly TranslateTransform _translateTransform = new();

    public ImageGalleryView()
    {
        InitializeComponent();
        ScrollViewer scrollViewer = this.FindControl<ScrollViewer>("ThumbnailsScrollViewer");
        scrollViewer.PointerWheelChanged += ScrollViewer_PointerWheelChanged;

        Viewbox imageViewbox = this.FindControl<Viewbox>("ImageViewbox");
        imageViewbox.PointerWheelChanged += ImageViewbox_PointerWheelChanged;

        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(_scaleTransform);
        transformGroup.Children.Add(_translateTransform);
        imageViewbox.RenderTransform = transformGroup;
    }

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
        if (DataContext is ImageGalleryViewModel viewModel)
        {
            viewModel.LoadMoreThumbnailsCommand.Execute(null);
        }
    }

    private void ImageViewbox_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (sender is not Viewbox) return;

        _currentScale = e.Delta.Y > 0 ? Math.Min(_currentScale + ScaleStep, MaxScale) : Math.Max(_currentScale - ScaleStep, MinScale);

        _scaleTransform.ScaleX = _currentScale;
        _scaleTransform.ScaleY = _currentScale;
        e.Handled = true;
    }

    private void ImageViewbox_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed &&
            !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        _isPanning = true;
        _lastPanPoint = e.GetPosition(this);
        e.Pointer.Capture((IInputElement)sender);
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
        if (!_isPanning) return;
        var currentPoint = e.GetPosition(this);
        var delta = currentPoint - _lastPanPoint;
        _lastPanPoint = currentPoint;

        _translateTransform.X += delta.X;
        _translateTransform.Y += delta.Y;
    }
    
    private void ResetImageViewBox(object? sender, RoutedEventArgs routedEventArgs)
    {
        _currentScale = 1.0;
        _scaleTransform.ScaleX = _currentScale;
        _scaleTransform.ScaleY = _currentScale;
        _translateTransform.X = 0;
        _translateTransform.Y = 0;
    }
    private void FlipHorizontally_Click(object? sender, RoutedEventArgs e)
    {
        _scaleTransform.ScaleX *= -1;
    }

    private void FlipVertically_Click(object? sender, RoutedEventArgs e)
    {
        _scaleTransform.ScaleY *= -1;
    }
}