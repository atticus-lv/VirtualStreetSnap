using System;
using System.IO;
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
    private readonly MainWindowViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = (MainWindowViewModel)DataContext!;

        ToggleButtonTopMost.IsCheckedChanged += (sender, e) => { Topmost = !Topmost; };
    }

    private void ToolBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!Equals(e.Source, ToolBar)) return;
        BeginMoveDrag(e);
    }

    private async void OnSaveImageButtonClick(object? sender, RoutedEventArgs e)
    {
        CompositionGuides.IsVisible = false;
        var currentScreen = Screens.Primary;
        var screenshot = await ScreenshotHelper.CaptureFullScreenAsync(currentScreen.Bounds);
        CompositionGuides.IsVisible = true;

        // calculate capture area
        var appScale = currentScreen.Scaling;
        var toolBarHeight = ToolBar.Bounds.Height;
        var borderCrop = 2 * appScale;
        var captureArea = CaptureArea.Bounds;
        captureArea = new Rect(captureArea.X + Position.X + borderCrop,
            captureArea.Y + Position.Y + toolBarHeight * appScale + borderCrop,
            captureArea.Width * appScale - borderCrop, captureArea.Height * appScale - borderCrop);

        var captureShot = ScreenshotHelper.CropImage(screenshot, captureArea);
        // save screenshot
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        var time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        // screenshot.Save(Path.Combine(path, $"FULL_{time}.png"));
        captureShot.Save(Path.Combine(path, $"CAP_{time}.png"));
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox comboBox || comboBox.SelectedItem is not SizeRadio selectedSizeRadio) return;

        var toolBarHeight = ToolBar.Bounds.Height;
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
        Height = contentHeight + toolBarHeight;
        Position = new PixelPoint((int)endX, (int)endY);
    }
}