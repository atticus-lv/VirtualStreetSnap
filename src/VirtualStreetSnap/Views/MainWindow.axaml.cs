using Avalonia.Controls;
using Avalonia.Input;

namespace VirtualStreetSnap.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // get screen width and height, then set window size 1/2
        var scale = Screens.Primary.Scaling;
        var currentScreen = Screens.Primary;
        Width = currentScreen.Bounds.Width / 2 / scale;
        Height = currentScreen.Bounds.Height / 2 / scale;
    }

    private void DragMoveWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!Equals(e.Source, ToolBar)) return;
        BeginMoveDrag(e);
    }
}