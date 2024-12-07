using System.ComponentModel;
using Avalonia.Media;

namespace VirtualStreetSnap.Models;

public class Settings
{
    public required string FilePrefix { get; set; } = "IMG";
    private string _saveDirectory;
    public event PropertyChangedEventHandler? PropertyChanged;

    public string SaveDirectory
    {
        get => _saveDirectory;
        set
        {
            if (_saveDirectory == value) return;
            _saveDirectory = value;
            OnPropertyChanged(nameof(SaveDirectory));
        }
    }

    public string Language { get; set; } = "en-US";

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Overlays
{
    public required Guides Guides { get; set; }
    public bool ShowFocusBorder { get; set; } = true;
    private int _focusBorderThickness = 10;

    public int FocusBorderThickness
    {
        get => _focusBorderThickness;
        set
        {
            if (_focusBorderThickness == value) return;
            _focusBorderThickness = value;
            OnPropertyChanged(nameof(FocusBorderThickness));
        }
    }

    private Color _focusBorderColor = Colors.Brown;

    public Color FocusBorderColor
    {
        get => _focusBorderColor;
        set
        {
            if (_focusBorderColor == value) return;
            _focusBorderColor = value;
            OnPropertyChanged(nameof(FocusBorderColor));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Guides
{
    public bool Grid { get; set; } = true;
    public bool Center { get; set; }
    public bool Ratio { get; set; }

    public float Opacity { get; set; } = 0.5f;
}

public class AppConfig
{
    public string Version { get; set; } = "1.0";
    public required Settings Settings { get; set; }
    public required Overlays Overlays { get; set; }
}