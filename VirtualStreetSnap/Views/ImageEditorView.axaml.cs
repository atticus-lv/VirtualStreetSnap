using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using VirtualStreetSnap.ViewModels;
using VirtualStreetSnap.ViewModels.ImageEditorLayer;

namespace VirtualStreetSnap.Views;

public partial class ImageEditorView : Window
{
    private LayerBaseViewModel? _dragItem;
    private Point _startPoint;

    public ImageEditorView()
    {
        InitializeComponent();
        var scale = Screens.Primary.Scaling;
        var currentScreen = Screens.Primary;
        Width = currentScreen.Bounds.Width / 2 / scale;
        Height = currentScreen.Bounds.Height / 2 / scale;

        LayerListBox.AddHandler(PointerPressedEvent, LayerListBox_OnPointerPressed, RoutingStrategies.Tunnel);
        LayerListBox.AddHandler(PointerReleasedEvent, LayerListBox_OnPointerRelease);
        LayerListBox.AddHandler(PointerMovedEvent, LayerListBox_OnPointerMove);
    }

    private LayerBaseViewModel? GetMouseOverItem(object? sender, PointerEventArgs e)
    {
        var point = e.GetPosition((Visual)sender);
        var visual = ((Visual)sender).GetVisualsAt(point).FirstOrDefault();
        var listBoxItem = visual?.GetLogicalAncestors().OfType<ListBoxItem>().FirstOrDefault();
        return listBoxItem?.DataContext as LayerBaseViewModel;
    }

    private void LayerListBox_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var dragItem = GetMouseOverItem(sender, e);
        if (dragItem == null)
        {
            GhostDragItem.IsVisible = false;
            return;
        }
        _startPoint = e.GetPosition(this);
        _dragItem = dragItem;
        var viewModel = DataContext as ImageEditorViewModel;
        viewModel.DragItemText = dragItem.Name;
        var currentPosition = e.GetPosition(this);
        Canvas.SetLeft(GhostDragItem, currentPosition.X - GhostDragItem.Width / 2);
        Canvas.SetTop(GhostDragItem, currentPosition.Y - GhostDragItem.Height / 2);
        Console.WriteLine($"Pressed on {dragItem}, at {GhostDragItem}");
    }

    private void LayerListBox_OnPointerMove(object? sender, PointerEventArgs e)
    {
        if (_dragItem == null) return;

        var currentPosition = e.GetPosition(this);
        var distance = Math.Sqrt(Math.Pow(currentPosition.X - _startPoint.X, 2) + Math.Pow(currentPosition.Y - _startPoint.Y, 2));

        // 设置一个阈值，例如10像素
        if (distance < 10) return;
        if (GhostDragItem.IsVisible == false)
        {
            GhostDragItem.IsVisible = true;
        }
        Canvas.SetLeft(GhostDragItem, currentPosition.X - GhostDragItem.Width / 2);
        Canvas.SetTop(GhostDragItem, currentPosition.Y - GhostDragItem.Height / 2);
    }


    private void LayerListBox_OnPointerRelease(object? sender, PointerEventArgs e)
    {
        if (_dragItem == null) return;
        var dropItem = GetMouseOverItem(sender, e);
        if (dropItem == null || dropItem == _dragItem)
        {
            GhostDragItem.IsVisible = false;
            _dragItem = null;
            return;
        };
        var viewModel = DataContext as ImageEditorViewModel;
        var targetIndex = viewModel?.LayerManager.Layers.IndexOf(dropItem);
        if (targetIndex == null) return;
        viewModel?.LayerManager.MoveLayer(_dragItem, targetIndex.Value);
        _dragItem = null;
        GhostDragItem.IsVisible = false;
    }

    private void AddLayerMenuButton_Click(object? sender, RoutedEventArgs e)
    {
        // Add a new layer to the image
        if (sender is not Button button) return;
        LayerTypeMenu.PlacementTarget = button;
        LayerTypeMenu.Open(button);
    }

    private void CloseButtonOnClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = (ImageEditorViewModel)DataContext;
        Close(viewModel.SaveImageToGalleryDirectory(true));
    }
}