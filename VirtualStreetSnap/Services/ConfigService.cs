using System;
using System.IO;
using Newtonsoft.Json;
using VirtualStreetSnap.Models;

namespace VirtualStreetSnap.Services;

public class ConfigService(string configFilePath)
{
    public AppConfig? LoadConfig()
    {
        if (!File.Exists(configFilePath))
            return NewDefaultConfig();

        try
        {
            var json = File.ReadAllText(configFilePath);
            return JsonConvert.DeserializeObject<AppConfig>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading config: {ex.Message}");
            return NewDefaultConfig();
        }
    }

    public void SaveConfig(AppConfig config)
    {
        try
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving config: {ex.Message}");
        }
    }

    private static AppConfig NewDefaultConfig()
    {
        return new AppConfig
        {
            Version = "0.0.1",
            Settings = new Settings
            {
                SaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Pictures", "VirtualStreetSnap"),
                FilePrefix = "IMG"
            },
            Overlays = new Overlays
            {
                Guides = new Guides
                {
                    Grid = true,
                    Center = false,
                    Ratio = false,
                    Opacity = 0.5f
                },
                Focus = true
            }
        };
    }
}