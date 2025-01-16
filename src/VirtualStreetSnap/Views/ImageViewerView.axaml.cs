using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using VirtualStreetSnap.Behaviors;

namespace VirtualStreetSnap.Views;

public partial class ImageViewerView : UserControl
{
    private ImageViewerBehavior? ImageViewerBehavior => 
        this.FindControl<StackPanel>("ViewBoxArea")?
        .GetValue(Interaction.BehaviorsProperty) is BehaviorCollection behaviors
            ? behaviors.OfType<ImageViewerBehavior>().FirstOrDefault()
            : null;

    public ImageViewerView()
    {
        InitializeComponent();
    }

    private void OnResetImageViewBox_Click(object? sender, RoutedEventArgs routedEventArgs)
    {
        ImageViewerBehavior?.ResetImageViewBox();
    }

    private void OnFlipHorizontally_Click(object? sender, RoutedEventArgs e)
    {
        ImageViewerBehavior?.FlipHorizontally();
    }

    private void OnFlipVertically_Click(object? sender, RoutedEventArgs e)
    {
        ImageViewerBehavior?.FlipVertically();
    }

    private void OnShowColorPicker_Click(object? sender, RoutedEventArgs e)
    {
        ImageViewerBehavior?.ShowColorPicker();
    }
}