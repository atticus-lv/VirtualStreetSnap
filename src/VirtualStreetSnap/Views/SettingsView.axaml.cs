using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using VirtualStreetSnap.ViewModels;

namespace VirtualStreetSnap.Views;

public partial class SettingsView : UserControl
{   
    private WindowNotificationManager _notificationManager;
    public SettingsView()
    {
        InitializeComponent();
    }
    
}