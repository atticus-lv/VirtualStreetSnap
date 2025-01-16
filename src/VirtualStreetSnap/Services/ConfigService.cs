using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using VirtualStreetSnap.Models;

namespace VirtualStreetSnap.Services;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AppConfig))]
internal partial class AppJsonContext : JsonSerializerContext
{
}

public class ConfigService
{
    private static readonly Lazy<AppConfig> _configInstance = new(() => LoadConfig());
    private static readonly string _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
    
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = AppJsonContext.Default,
        Converters = { new ColorJsonConverter() }
    };

    private ConfigService() { }

    public static AppConfig Instance => _configInstance.Value;

    private static AppConfig LoadConfig()
    {
        if (!File.Exists(_configFilePath))
            return NewDefaultConfig();

        try
        {
            var json = File.ReadAllText(_configFilePath);
            return JsonSerializer.Deserialize<AppConfig>(json, _jsonOptions) ?? NewDefaultConfig();
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
            var json = JsonSerializer.Serialize(Instance, _jsonOptions);
            File.WriteAllText(_configFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving config: {ex.Message}");
        }
    }
    
    public static void SaveIfNotExists()
    {
        if (!File.Exists(_configFilePath))
            SaveConfig();
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
                ShowFocusBorder = true
            }
        };
    }
}