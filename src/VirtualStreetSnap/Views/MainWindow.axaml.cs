using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

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
#if LINUX 
        // Hide title bar on Linux
        SystemDecorations = SystemDecorations.BorderOnly;
#endif
    }

    private void DragMoveWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!Equals(e.Source, ToolBar)) return;
        BeginMoveDrag(e);
    }

    private void ToggleTopMost_OnClick(object? sender, RoutedEventArgs e)
    {
        Topmost = !Topmost;
    }
}