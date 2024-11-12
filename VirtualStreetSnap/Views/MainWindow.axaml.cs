using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using  VirtualStreetSnap.Models;
using  VirtualStreetSnap.Services;
using  VirtualStreetSnap.ViewModels;

namespace  VirtualStreetSnap.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    
    public MainWindow()
    {
        InitializeComponent();
        _viewModel = (MainWindowViewModel)DataContext;
    }

    private void BottomBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!Equals(e.Source, BottomBar)) return;
        BeginMoveDrag(e);
    }


    private async void OnSaveImageButtonClick(object? sender, RoutedEventArgs e)
    {
        CaptureBorder.IsVisible = false;
        var currentScreen = Screens.Primary;
        var screenshot = await ScreenshotHelper.CaptureFullScreenAsync(currentScreen.Bounds);
        CaptureBorder.IsVisible = true;

        var captureArea = CaptureArea.Bounds;
        captureArea = new Rect(captureArea.X + Position.X, captureArea.Y + Position.Y, captureArea.Width,
            captureArea.Height);
        var captureShot = ScreenshotHelper.CropImage(screenshot, captureArea);
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        var time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        captureShot.Save(Path.Combine(path, $"CAP_{time}.png"));
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox comboBox || comboBox.SelectedItem is not SizeRadio selectedSizeRadio) return;

        var bottomBarHeight = BottomBar.Bounds.Height;
        var contentHeight = (int)(Height - bottomBarHeight);
        var hiddenTitleBarHeight = (int)(Height - contentHeight);

        var width = selectedSizeRadio.getWidth(contentHeight);
        var endX = Position.X + (Width - width) / 2;
        var endY = Position.Y + Height - contentHeight - bottomBarHeight - hiddenTitleBarHeight;

        Width = width;
        Height = contentHeight + bottomBarHeight;
        Position = new PixelPoint((int)endX, (int)endY);
    }
}