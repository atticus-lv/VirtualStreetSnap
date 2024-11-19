namespace VirtualStreetSnap.Models;

public class Settings
{
    public required string SaveDirectory { get; set; }
    public required string FilePrefix { get; set; } = "IMG";
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
    public required string Version { get; set; } = "0.0.1";
    public required Settings Settings { get; set; }
    public required Overlays Overlays { get; set; }
}