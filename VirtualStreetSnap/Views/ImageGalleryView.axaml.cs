using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace VirtualStreetSnap.Views;

public partial class ImageGalleryView : UserControl
{
    public ImageGalleryView()
    {
        InitializeComponent();
        ScrollViewer scrollViewer = this.FindControl<ScrollViewer>("ThumbnailsScrollViewer");
        scrollViewer.PointerWheelChanged += ScrollViewer_PointerWheelChanged;
    }

    private void ScrollViewer_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;
        double scrollFactor = 30; // Adjust this factor to increase or decrease the scroll distance
        scrollViewer.Offset = new Vector(scrollViewer.Offset.X - e.Delta.Y * scrollFactor, scrollViewer.Offset.Y);
        e.Handled = true;
    }
}