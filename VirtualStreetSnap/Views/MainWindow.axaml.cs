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
        // OnTopMostCheckbox.IsCheckedChanged += (sender, e) => { Topmost = !Topmost; };
        ToggleGalleryButton.IsCheckedChanged += (sender, e) =>
        {
            if (ToggleGalleryButton.IsChecked == false) return;
            var viewModel = GalleryView.DataContext as ImageGalleryViewModel;
            viewModel?.UpdateThumbnails();
        };
    }

    private void ToolBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!Equals(e.Source, ToolBar)) return;
        BeginMoveDrag(e);
    }

    private async void SnapshotButton_Click(object? sender, RoutedEventArgs e)
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

        var _viewModel = (MainWindowViewModel)DataContext;
        var saveDir = _viewModel.Config.Settings.SaveDirectory;
        if (!Path.Exists(saveDir)) Directory.CreateDirectory(saveDir);
        var filePrefix = _viewModel.Config.Settings.FilePrefix;
        filePrefix = new string(filePrefix.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());

        var time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        // screenshot.Save(Path.Combine(path, $"FULL_{time}.png"));
        captureShot.Save(Path.Combine(saveDir, $"{filePrefix}_{time}.png"));
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

    private void ToggleGalleryButton_Click(object sender, RoutedEventArgs e)
    {
        if (ToggleGalleryButton.IsChecked == true) ToggleSettingsButton.IsChecked = false;
    }

    private void ToggleSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        if (ToggleSettingsButton.IsChecked == true) ToggleGalleryButton.IsChecked = false;
    }
}