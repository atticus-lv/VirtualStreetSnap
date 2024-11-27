using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // When the app is in preview mode, the user can't take a screenshot

    [ObservableProperty]
    private bool _isGalleryView;

    [ObservableProperty]
    private bool _isSettingsView;

    [ObservableProperty]
    private SizeRadio? _selectedSizeRadio;

    [ObservableProperty]
    private AppConfig _config = ConfigService.Instance;


    public ObservableCollection<LanguageModel> LanguageModels { get; } =
    [
        new("English", "en-US"),
        new("中文", "zh-CN"),
    ];

    public ObservableCollection<SizeRadio> RadioItems { get; } =
    [
        new("16:9"),
        new("4:3"),
        new("3:2"),
        new("1:1"),
        new("3:4"),
        new("9:16")
    ];

    public MainWindowViewModel()
    {   
        Localizer.Localizer.Instance.LoadLanguage("zh-CN");
        SelectedSizeRadio = RadioItems.First();
        ConfigService.SaveIfNotExists();
    }

    [RelayCommand]
    public void OnCloseButtonClick()
    {
        if (Design.IsDesignMode) return;
        // Save the config before closing the app
        Config.Version = "1.0";
        ConfigService.SaveConfig();
        Environment.Exit(0);
    }
}