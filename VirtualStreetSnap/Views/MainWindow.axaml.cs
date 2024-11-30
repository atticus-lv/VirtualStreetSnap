using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Services;
using VirtualStreetSnap.ViewModels;

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

        // OnTopMostCheckbox.IsCheckedChanged += (sender, e) => { Topmost = !Topmost; };
    }

    private void ToolBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!Equals(e.Source, ToolBar)) return;
        BeginMoveDrag(e);
    }


    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox comboBox || comboBox.SelectedItem is not SizeRadio selectedSizeRadio) return;

        var toolBarHeight = 0;
        var contentHeight = (int)(Height - toolBarHeight);
        var hiddenTitleBarHeight = (int)(Height - contentHeight);

        var width = selectedSizeRadio.GetWidth(contentHeight);
        if (width < MinWidth)
        {
            width = (int)MinWidth;
            contentHeight = selectedSizeRadio.GetHeight(width);
        }

        var endX = Position.X + (Width - width) / 2;
        var endY = Position.Y + Height - contentHeight - toolBarHeight - hiddenTitleBarHeight;

        Width = width;
        Height = contentHeight;
        Position = new PixelPoint((int)endX, (int)endY);
    }
}