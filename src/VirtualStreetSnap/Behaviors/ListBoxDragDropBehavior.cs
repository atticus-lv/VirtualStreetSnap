using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using VirtualStreetSnap.ViewModels;
using VirtualStreetSnap.ViewModels.ImageEditorLayer;

namespace VirtualStreetSnap.Behaviors;

public class ListBoxDragDropBehavior : Behavior<ListBox>
{
    private LayerBaseViewModel? _dragItem;
    private Point _startPoint;
    private LayerBaseViewModel? _previousDropItem;
    private bool _isDragging;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is null) return;

        AssociatedObject.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        AssociatedObject.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject is null) return;

        AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _dragItem = GetMouseOverItem(sender, e);
        if (_dragItem == null)
        {
            UpdateGhostDragItemVisibility(false);
            return;
        }

        _startPoint = e.GetPosition(AssociatedObject.GetVisualRoot() as Visual);
        var viewModel = AssociatedObject.DataContext as ImageEditorViewModel;
        if (viewModel != null)
        {
            viewModel.DragItemText = _dragItem.Name;
        }

        UpdateGhostDragItemPosition(e.GetPosition(AssociatedObject.GetVisualRoot() as Visual));
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
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

        if (IsPointerOutsideListBox(e) || _dragItem == null)
        {
            AssociatedObject.Cursor = new Cursor(StandardCursorType.No);
            UpdateGhostDragItemVisibility(false);
            return;
        }

        var currentPosition = e.GetPosition(AssociatedObject.GetVisualRoot() as Visual);
        if (!IsDragThresholdExceeded(currentPosition)) return;

        if (!_isDragging)
        {
            _isDragging = true;
            UpdateGhostDragItemVisibility(true);
        }

        if (_isDragging)
        {
            AssociatedObject.Cursor = new Cursor(StandardCursorType.DragMove);
        }

        UpdateGhostDragItemPosition(currentPosition);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragItem == null) return;

        var dropItem = GetMouseOverItem(sender, e);
        if (dropItem == null || dropItem == _dragItem)
        {
            ResetDragState();
            return;
        }

        var viewModel = AssociatedObject.DataContext as ImageEditorViewModel;
        var targetIndex = viewModel?.LayerManager.Layers.IndexOf(dropItem);
        if (targetIndex.HasValue)
        {
            viewModel?.LayerManager.MoveLayer(_dragItem, targetIndex.Value);
        }

        ResetDragState();
    }

    private LayerBaseViewModel? GetMouseOverItem(object? sender, PointerEventArgs e)
    {
        var point = e.GetPosition((Visual)sender);
        var visual = ((Visual)sender).GetVisualsAt(point).FirstOrDefault();
        if (visual is Border) return null;
        var listBoxItem = visual?.GetLogicalAncestors().OfType<ListBoxItem>().FirstOrDefault();
        return listBoxItem?.DataContext as LayerBaseViewModel;
    }

    private bool IsPointerOutsideListBox(PointerEventArgs e)
    {
        var position = e.GetPosition(AssociatedObject);
        return position.X < 0 || position.Y < 0 ||
               position.X > AssociatedObject.Bounds.Width ||
               position.Y > AssociatedObject.Bounds.Height;
    }

    private bool IsDragThresholdExceeded(Point currentPosition)
    {
        var deltaX = currentPosition.X - _startPoint.X;
        var deltaY = currentPosition.Y - _startPoint.Y;
        var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        return distance >= 10;
    }

    private void ResetDragState()
    {
        UpdateGhostDragItemVisibility(false);
        _dragItem = null;
        _isDragging = false;
        AssociatedObject.Cursor = Cursor.Default;
        if (_previousDropItem == null) return;
        _previousDropItem.IsDropTarget = false;
        _previousDropItem = null;
    }

    private void UpdateGhostDragItemVisibility(bool isVisible)
    {
        var window = AssociatedObject.GetVisualRoot() as Window;
        if (window == null) return;

        var ghostDragItem = window.FindControl<Border>("GhostDragItem");
        if (ghostDragItem != null)
        {
            ghostDragItem.IsVisible = isVisible;
        }
    }

    private void UpdateGhostDragItemPosition(Point position)
    {
        var window = AssociatedObject.GetVisualRoot() as Window;
        if (window == null) return;

        var ghostDragItem = window.FindControl<Border>("GhostDragItem");
        if (ghostDragItem != null)
        {
            Canvas.SetLeft(ghostDragItem, position.X + 20);
            Canvas.SetTop(ghostDragItem, position.Y - ghostDragItem.Height / 2);
        }
    }
}