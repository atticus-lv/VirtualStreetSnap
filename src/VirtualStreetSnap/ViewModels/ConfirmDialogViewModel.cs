using CommunityToolkit.Mvvm.ComponentModel;

namespace VirtualStreetSnap.ViewModels;

public partial class ConfirmDialogViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "Title";

    [ObservableProperty]
    private string _message = "This is a very important message. Which is very important.";
    
    [ObservableProperty]
    private string _confirmButtonText = "Confirm";
    
    [ObservableProperty]
    private string _cancelButtonText = "Cancel";
    
    [ObservableProperty]
    private int _width = 300;
    
    [ObservableProperty]
    private int _height = 200;
}