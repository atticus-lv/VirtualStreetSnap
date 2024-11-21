using System.ComponentModel;

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

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}

public class Overlays
{
    public required Guides Guides { get; set; }
    public bool Focus { get; set; } = true;
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
    public string Version { get; set; } = "0.0.2";
    public required Settings Settings { get; set; }
    public required Overlays Overlays { get; set; }
}