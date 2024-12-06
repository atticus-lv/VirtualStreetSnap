using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace VirtualStreetSnap.Views;

public partial class ConfirmDialog : Window
{
    public ConfirmDialog()
    {
        InitializeComponent();
        // Closed += (sender, e) => { Close(); };
    }

    private void Confirm_OnClick(object? sender, RoutedEventArgs e)
    {   
        if (Design.IsDesignMode) return;
        Close(true);
    }
    
    private void Cancel_OnClick(object? sender, RoutedEventArgs e)
    {   
        if (Design.IsDesignMode) return;
        Close(false);
    }
}