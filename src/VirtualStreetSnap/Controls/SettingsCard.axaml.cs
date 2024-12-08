using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace VirtualStreetSnap.Controls;

public partial class SettingsCard : ContentControl
{
    public static readonly StyledProperty<string> SettingNameProperty =
        AvaloniaProperty.Register<SettingsCard, string>(nameof(SettingName));

    public string SettingName
    {
        get { return GetValue(SettingNameProperty); }
        set { SetValue(SettingNameProperty, value); }
    }

    public SettingsCard()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}