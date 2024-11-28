using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using VirtualStreetSnap.ViewModels;

namespace VirtualStreetSnap.Views;

public partial class ImageEditorView : Window
{
    public ImageEditorView()
    {
        InitializeComponent();
            
    }
    
    private void AddLayerMenuButton_Click(object? sender, RoutedEventArgs e)
    {
        // Add a new layer to the image
        if (sender is not Button button) return;
        LayerTypeMenu.PlacementTarget = button;
        LayerTypeMenu.Open(button);
    }
    
    private void ToolBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!Equals(e.Source, ToolBar)) return;
        BeginMoveDrag(e);
    }
    
    private void CloseButtonOnClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = (ImageEditorViewModel)DataContext;
        Close(viewModel.SaveImageToGalleryDirectory(true));
    }
    
}