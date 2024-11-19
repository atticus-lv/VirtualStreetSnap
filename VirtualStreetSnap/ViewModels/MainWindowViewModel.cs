using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

    [ObservableProperty]
    private SizeRadio? _selectedSizeRadio;

    [ObservableProperty]
    private bool _showFocusBorder = true;

    [ObservableProperty]
    private bool _showGuideLinesGrid = true;

    [ObservableProperty]
    private bool _showGuideLinesCenter;

    [ObservableProperty]
    private bool _showGuideLinesRatio;

    [ObservableProperty]
    private float _guideLinesOpacity = 0.5f;

    [ObservableProperty]
    private Color _borderColor = Colors.Brown;

    [ObservableProperty]
    private AppConfig _config;

    [ObservableProperty]
    private string _filePrefix = "IMG";

    [ObservableProperty]
    private string _saveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Pictures", "VirtualStreetSnap");

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
        SelectedSizeRadio = RadioItems.First();
        Console.WriteLine(configFilePath);
        var configService = new ConfigService(configFilePath);
        Config = configService.LoadConfig();
        if (Config is null) return;
        ShowGuideLinesGrid = Config.Overlays.Guides.Grid;
        ShowGuideLinesCenter = Config.Overlays.Guides.Center;
        ShowGuideLinesRatio = Config.Overlays.Guides.Ratio;
        ShowFocusBorder = Config.Overlays.Focus;
        SaveDirectory = Config.Settings.SaveDirectory;
        FilePrefix = Config.Settings.FilePrefix;
        // Set the default values if the config is not existing
        configService.SaveConfig(Config);
    }

    [RelayCommand]
    public void OnCloseButtonClick()
    {
        if (Design.IsDesignMode) return;
        // Save the config before closing the app
        Config.Overlays.Guides.Grid = ShowGuideLinesGrid;
        Config.Overlays.Guides.Center = ShowGuideLinesCenter;
        Config.Overlays.Guides.Ratio = ShowGuideLinesRatio;
        Config.Overlays.Focus = ShowFocusBorder;
        Config.Settings.SaveDirectory = SaveDirectory;
        Config.Settings.FilePrefix = FilePrefix;
        var configService = new ConfigService(configFilePath);
        configService.SaveConfig(Config);
        Environment.Exit(0);
    }

    public async Task ChangeDir(string parmName)
    {
        var task = parmName switch
        {
            "SaveDirectory" => this.ChangeDirectory(value => SaveDirectory = value, "Select Directory"),
            _ => Task.CompletedTask
        };
        await task;
    }
}