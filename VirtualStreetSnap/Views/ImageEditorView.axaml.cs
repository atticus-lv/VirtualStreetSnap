using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

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
}