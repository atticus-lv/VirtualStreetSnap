using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Services;
using VirtualStreetSnap.Views;

namespace VirtualStreetSnap.ViewModels;

public class PagesModel
{
    public PagesModel(string name, UserControl page, string? iconKey = null)
    {
        Name = name;
        Page = page;
        if (iconKey == null) return;
        Application.Current!.TryFindResource(iconKey, out var res);
        ItemIcon = (StreamGeometry)res!;
    }

    public string Name { get; set; }
    public UserControl Page { get; set; }
    public StreamGeometry? ItemIcon { get; }
}

public partial class MainWindowViewModel : ViewModelBase
{
    // When the app is in preview mode, the user can't take a screenshot

    [ObservableProperty]
    private PagesModel? _currentPage;

    public ObservableCollection<PagesModel> Pages { get; } =
    [
        new("SnapShot", new SnapShotView(), "CameraRegular"),
        new("Gallery", new ImageGalleryView(), "ImageCopyRegular"),
        new("Settings", new SettingsView(), "Settings")
    ];

    partial void OnCurrentPageChanged(PagesModel value)
    {
        CurrentPage = value;
        if (value.Name != "Gallery") return;
        var viewModel = value.Page.DataContext as ImageGalleryViewModel;
        viewModel?.UpdateThumbnails();
    }


    [ObservableProperty]
    private AppConfig _config = ConfigService.Instance;


    public MainWindowViewModel()
    {
        CurrentPage = Pages.First();
        var snapView = Pages[0].Page as SnapShotView;
        snapView?.FixWindowSize();
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