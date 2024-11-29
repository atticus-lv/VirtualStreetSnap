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
    private LayerBaseViewModel? _previousDropItem;

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
    
    private bool IsPointerOutsideLayerListBox(PointerEventArgs e)
    {
        var position = e.GetPosition(LayerListBox);
        return position.X < 0 || position.Y < 0 || position.X > LayerListBox.Bounds.Width || position.Y > LayerListBox.Bounds.Height;
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
        _dragItem = GetMouseOverItem(sender, e);
        if (_dragItem == null)
        {
            GhostDragItem.IsVisible = false;
            return;
        }
        _startPoint = e.GetPosition(this);
        var viewModel = DataContext as ImageEditorViewModel;
        viewModel.DragItemText = _dragItem.Name;
        UpdateGhostDragItemPosition(e.GetPosition(this));
        Console.WriteLine($"Pressed on {_dragItem}, at {GhostDragItem}");
    }

    private void LayerListBox_OnPointerMove(object? sender, PointerEventArgs e)
    {
        if (_dragItem == null) return;

        var dropItem = GetMouseOverItem(sender, e);
        if (dropItem != null && dropItem != _dragItem)
        {
            if (_previousDropItem != null && _previousDropItem != dropItem)
            {
                _previousDropItem.IsDropTarget = false;
            }
            dropItem.IsDropTarget = true;
            _previousDropItem = dropItem;
        }
        else
        {
            if (_previousDropItem != null)
            {
                _previousDropItem.IsDropTarget = false;
                _previousDropItem = null;
            }
        }

        if (IsPointerOutsideLayerListBox(e))
        {
            ResetDragState();
            return;
        }

        var currentPosition = e.GetPosition(this);
        if (!IsDragThresholdExceeded(currentPosition)) return;
        GhostDragItem.IsVisible = true;
        UpdateGhostDragItemPosition(currentPosition);
    }

    private void LayerListBox_OnPointerRelease(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragItem == null) return;

        var dropItem = GetMouseOverItem(sender, e);
        if (dropItem == null || dropItem == _dragItem)
        {
            ResetDragState();
            return;
        }

        var viewModel = DataContext as ImageEditorViewModel;
        var targetIndex = viewModel?.LayerManager.Layers.IndexOf(dropItem);
        if (targetIndex.HasValue)
        {
            viewModel.LayerManager.MoveLayer(_dragItem, targetIndex.Value);
        }
        ResetDragState();
    }

    private void AddLayerMenuButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            LayerTypeMenu.PlacementTarget = button;
            LayerTypeMenu.Open(button);
        }
    }

    private void CloseButtonOnClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = (ImageEditorViewModel)DataContext;
        Close(viewModel.SaveImageToGalleryDirectory(true));
    }

    private void UpdateGhostDragItemPosition(Point position)
    {
        Canvas.SetLeft(GhostDragItem, position.X + 20);
        Canvas.SetTop(GhostDragItem, position.Y - GhostDragItem.Height / 2);
    }

    private bool IsDragThresholdExceeded(Point currentPosition)
    {
        var distance = Math.Sqrt(Math.Pow(currentPosition.X - _startPoint.X, 2) + Math.Pow(currentPosition.Y - _startPoint.Y, 2));
        return distance >= 10;
    }

    private void ResetDragState()
    {
        GhostDragItem.IsVisible = false;
        _dragItem = null;
        if (_previousDropItem == null) return;
        _previousDropItem.IsDropTarget = false;
        _previousDropItem = null;
    }
}