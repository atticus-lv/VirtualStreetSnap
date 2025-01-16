using System.ComponentModel;
using Avalonia.Media;
using System.Text.Json.Serialization;
using VirtualStreetSnap.Services;

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

    [JsonConverter(typeof(ColorJsonConverter))]
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

public class Guides : INotifyPropertyChanged
{
    private bool _grid = true;
    private bool _center;
    private bool _ratio;
    private float _opacity = 0.5f;

    public bool Grid
    {
        get => _grid;
        set
        {
            if (_grid == value) return;
            _grid = value;
            OnPropertyChanged(nameof(Grid));
        }
    }

    public bool Center
    {
        get => _center;
        set
        {
            if (_center == value) return;
            _center = value;
            OnPropertyChanged(nameof(Center));
        }
    }

    public bool Ratio
    {
        get => _ratio;
        set
        {
            if (_ratio == value) return;
            _ratio = value;
            OnPropertyChanged(nameof(Ratio));
        }
    }

    public float Opacity
    {
        get => _opacity;
        set
        {
            if (_opacity == value) return;
            _opacity = value;
            OnPropertyChanged(nameof(Opacity));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class AppConfig
{
    public string Version { get; set; } = "1.0";
    public required Settings Settings { get; set; }
    public required Overlays Overlays { get; set; }
}