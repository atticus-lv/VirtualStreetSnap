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
using Image = Avalonia.Controls.Image;

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


        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(_scaleTransform);
        transformGroup.Children.Add(_translateTransform);
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
        // if (!(scrollViewer.Offset.X >= scrollViewer.Extent.Width - scrollViewer.Viewport.Width)) return;
        if (!(scrollViewer.Offset.Y >= scrollViewer.Extent.Height - scrollViewer.Viewport.Height)) return;
        if (DataContext is ImageGalleryViewModel viewModel) viewModel.LoadMoreThumbnailsCommand.Execute(null);
    }
}