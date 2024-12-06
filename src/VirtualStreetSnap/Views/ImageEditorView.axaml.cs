using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using VirtualStreetSnap.ViewModels;
using VirtualStreetSnap.ViewModels.ImageEditorLayer;

namespace VirtualStreetSnap.Views;

public partial class ImageEditorView : UserControl
{
    private LayerBaseViewModel? _dragItem;
    private Point _startPoint;
    private LayerBaseViewModel? _previousDropItem;

    public ImageEditorView()
    {
        InitializeComponent();

        LayerListBox.AddHandler(PointerPressedEvent, LayerListBox_OnPointerPressed, RoutingStrategies.Tunnel);
        LayerListBox.AddHandler(PointerReleasedEvent, LayerListBox_OnPointerRelease);
        LayerListBox.AddHandler(PointerMovedEvent, LayerListBox_OnPointerMove);
    }

    

    private bool IsPointerOutsideLayerListBox(PointerEventArgs e)
    {
        var position = e.GetPosition(LayerListBox);
        return position.X < 0 || position.Y < 0 || position.X > LayerListBox.Bounds.Width ||
               position.Y > LayerListBox.Bounds.Height;
    }

    private LayerBaseViewModel? GetMouseOverItem(object? sender, PointerEventArgs e)
    {
        var point = e.GetPosition((Visual)sender);
        var visual = ((Visual)sender).GetVisualsAt(point).FirstOrDefault();
        if (visual is Border) return null; // ignore border of checkbox
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

        if (IsPointerOutsideLayerListBox(e) || _dragItem == null)
        {
            Cursor = new Cursor(StandardCursorType.No);
            GhostDragItem.IsVisible = false;
            return;
        }

        var currentPosition = e.GetPosition(this);
        if (!IsDragThresholdExceeded(currentPosition)) return;
        if (!GhostDragItem.IsVisible) GhostDragItem.IsVisible = true;
        if (GhostDragItem.IsVisible)
        {
            Cursor = new Cursor(StandardCursorType.DragMove);
        }

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

    private void PopupLayerMenuButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        LayerTypeMenu.PlacementTarget = button;
        LayerTypeMenu.Open(button);
    }



    private void UpdateGhostDragItemPosition(Point position)
    {
        Canvas.SetLeft(GhostDragItem, position.X + 20);
        Canvas.SetTop(GhostDragItem, position.Y - GhostDragItem.Height / 2);
    }

    private bool IsDragThresholdExceeded(Point currentPosition)
    {
        var distance = Math.Sqrt(Math.Pow(currentPosition.X - _startPoint.X, 2) +
                                 Math.Pow(currentPosition.Y - _startPoint.Y, 2));
        return distance >= 10;
    }

    private void ResetDragState()
    {
        GhostDragItem.IsVisible = false;
        _dragItem = null;
        Cursor = Cursor.Default;
        if (_previousDropItem == null) return;
        _previousDropItem.IsDropTarget = false;
        _previousDropItem = null;
    }


    public void SaveImageOverwrite(object? sender, RoutedEventArgs routedEventArgs)
    {
        var viewModel = (ImageEditorViewModel)DataContext;
        viewModel.SaveImageToGalleryDirectory(false);
    }

    public void SaveImageCopy(object? sender, RoutedEventArgs routedEventArgs)
    {
        var viewModel = (ImageEditorViewModel)DataContext;
        viewModel.SaveImageToGalleryDirectory(true);
    }

    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;
        var viewModel = (ImageEditorViewModel)DataContext;
        Console.WriteLine(menuItem.Header.ToString());
        // get real name instead of data passing localized string
        var dataContext = menuItem.GetLogicalAncestors().OfType<Control>()
            .FirstOrDefault(c => c.DataContext is LayerTypeItem)
            ?.DataContext as LayerTypeItem;
        viewModel.AddLayer(dataContext?.LayerName);
    }
}