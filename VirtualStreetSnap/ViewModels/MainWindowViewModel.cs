using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualStreetSnap.Models;

namespace VirtualStreetSnap.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private SizeRadio? _selectedSizeRadio;

    [ObservableProperty]
    private Color _borderColor = Colors.Brown;

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
    }


    [RelayCommand]
    public void OnCloseButtonClick()
    {
        if (Design.IsDesignMode) return;
        Environment.Exit(0);
    }
}