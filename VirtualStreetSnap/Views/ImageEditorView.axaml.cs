using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using VirtualStreetSnap.ViewModels;
using VirtualStreetSnap.ViewModels.ImageEditorLayer;

namespace VirtualStreetSnap.Views;

public partial class ImageEditorView : Window
{
    private LayerBaseViewModel? _dragItem;

    public ImageEditorView()
    {
        InitializeComponent();
        var scale = Screens.Primary.Scaling;
        var currentScreen = Screens.Primary;
        Width = currentScreen.Bounds.Width / 2 / scale;
        Height = currentScreen.Bounds.Height / 2 / scale;

        LayerListBox.AddHandler(PointerPressedEvent, LayerListBox_OnPointerPressed, RoutingStrategies.Tunnel);
        LayerListBox.AddHandler(PointerReleasedEvent, LayerListBox_OnPointerRelease);
    }

    private void LayerListBox_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetPosition((Visual)sender);
        var visual = ((Visual)sender).GetVisualsAt(point).FirstOrDefault();
        if (visual == null) return;
        var listBoxItem = visual.GetLogicalAncestors().OfType<ListBoxItem>().FirstOrDefault();
        // Console.WriteLine($"Pointer is over: {listBoxItem}");
        var dragItem = listBoxItem?.DataContext as LayerBaseViewModel;
        if (dragItem == null) return;
        _dragItem = dragItem;
        // set mouse drag effect
        
    }

    private void LayerListBox_OnPointerMove(object? sender, PointerPressedEventArgs e)
    {
        if (_dragItem == null) return;
        var point = e.GetPosition((Visual)sender);
        var listboxBounds = LayerListBox.Bounds;
        
        if (point is not { X: >= 0, Y: >= 0 }) return;
        if (!(point.X < listboxBounds.Width) || !(point.Y < listboxBounds.Height)) return;
        
        Canvas.SetLeft(VisualDragItem, e.GetPosition(this).X);
        Canvas.SetTop(VisualDragItem, e.GetPosition(this).Y);
        VisualDragItem.IsVisible = true;
    }

    private void LayerListBox_OnPointerRelease(object? sender, PointerEventArgs e)
    {
        if (_dragItem == null) return;
        var point = e.GetPosition((Visual)sender);
        var visual = ((Visual)sender).GetVisualsAt(point).FirstOrDefault();
        if (visual == null) return;
        var listBoxItem = visual.GetLogicalAncestors().OfType<ListBoxItem>().FirstOrDefault();
        if (listBoxItem == null || listBoxItem.DataContext == _dragItem) return;
        var dropItem = listBoxItem.DataContext as LayerBaseViewModel;
        if (dropItem == null) return;

        var viewModel = DataContext as ImageEditorViewModel;
        var targetIndex = viewModel?.LayerManager.Layers.IndexOf(dropItem);
        if (targetIndex == null) return;
        viewModel?.LayerManager.MoveLayer(_dragItem, targetIndex.Value);
        _dragItem = null;
        VisualDragItem.IsVisible = false;
        
    }

    private void AddLayerMenuButton_Click(object? sender, RoutedEventArgs e)
    {
        // Add a new layer to the image
        if (sender is not Button button) return;
        LayerTypeMenu.PlacementTarget = button;
        LayerTypeMenu.Open(button);
    }
}