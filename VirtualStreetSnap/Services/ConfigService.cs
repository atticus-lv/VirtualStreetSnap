using System;
using System.IO;
using Newtonsoft.Json;
using VirtualStreetSnap.Models;

namespace VirtualStreetSnap.Services;

public class ConfigService
{
    private static readonly Lazy<AppConfig> _configInstance = new(() => LoadConfig());
    private static readonly string _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

    private ConfigService() { }

    public static AppConfig Instance => _configInstance.Value;

    private static AppConfig LoadConfig()
    {
        if (!File.Exists(_configFilePath))
            return NewDefaultConfig();

        try
        {
            var json = File.ReadAllText(_configFilePath);
            return JsonConvert.DeserializeObject<AppConfig>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading config: {ex.Message}");
            return NewDefaultConfig();
        }
    }

    public static void SaveConfig()
    {
        try
        {
            var json = JsonConvert.SerializeObject(Instance, Formatting.Indented);
            File.WriteAllText(_configFilePath, json);
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
            Version = "1.0",
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