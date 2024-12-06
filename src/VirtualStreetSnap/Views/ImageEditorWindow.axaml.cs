using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace VirtualStreetSnap.Views;

public partial class ImageEditorWindow : Window
{
    public ImageEditorWindow()
    {
        InitializeComponent();
        CloseWindowButton.Click += (sender, e) => { Hide(); };

        var scale = Screens.Primary.Scaling;
        var currentScreen = Screens.Primary;
        Width = currentScreen.Bounds.Width / 2 / scale;
        Height = currentScreen.Bounds.Height / 2 / scale;
    }

    private void ToolBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void CloseButtonOnClick(object? sender, RoutedEventArgs e)
    { }
}