using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using VirtualStreetSnap.ViewModels;
using VirtualStreetSnap.ViewModels.ImageEditorLayer;

namespace VirtualStreetSnap.Views;

public partial class ImageEditorView : UserControl
{
    public ImageEditorView()
    {
        InitializeComponent();
    }

    private void PopupLayerMenuButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        LayerTypeMenu.PlacementTarget = button;
        LayerTypeMenu.Open(button);
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