using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private AppConfig _config = ConfigService.Instance;

    [ObservableProperty]
    private string _appVersion = "Unknown";


    [ObservableProperty]
    private string _filePrefix = "IMG";

    [ObservableProperty]
    private Color _focusBorderColor = Colors.Brown;

    [ObservableProperty]
    private int _focusBorderThickness = 10;

    [ObservableProperty]
    private LanguageModel _selectedLanguage;

    [ObservableProperty]
    private string _saveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Pictures", "VirtualStreetSnap");

    public ObservableCollection<LanguageModel> LanguageModels { get; } =
    [
        new("English", "en-US"),
        new("中文", "zh-CN"),
    ];

    public SettingsViewModel()
    {
        SaveDirectory = Config.Settings.SaveDirectory;
        FilePrefix = Config.Settings.FilePrefix;
        SelectedLanguage = LanguageModels.FirstOrDefault(x => x.Identifier == Config.Settings.Language) ??
                           LanguageModels.First();
        FocusBorderColor = Config.Overlays.FocusBorderColor;
        FocusBorderThickness = Config.Overlays.FocusBorderThickness;
        // Set the default values if the config is not existing
        ConfigService.SaveConfig();
        GetFileVersion();
    }


    public void GetFileVersion()
    {
#if WINDOWS
        var exeDir = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
        // get all the exe files in the directory
        var exePath = exeDir != null ? Directory.GetFiles(exeDir, "*.exe").FirstOrDefault() : null;
        if (exePath == null) return;
        var version = FileVersionInfo.GetVersionInfo(exePath).FileVersion;
        if (version == null) return;
        AppVersion = version;
#elif OSX
        // 尝试多个可能的路径
        var possiblePaths = new[]
        {
            // 发布环境路径
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Info.plist"),
            // 开发环境路径
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Contents", "Info.plist"),
            // 直接在当前目录查找
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Info.plist")
        };

        foreach (var bundlePath in possiblePaths)
        {
            if (!File.Exists(bundlePath)) continue;
            
            try
            {
                var plistContent = File.ReadAllText(bundlePath);
                // 简单解析plist文件获取版本号
                var versionStart = plistContent.IndexOf("<key>CFBundleShortVersionString</key>");
                if (versionStart != -1)
                {
                    var versionValueStart = plistContent.IndexOf("<string>", versionStart) + 8;
                    var versionValueEnd = plistContent.IndexOf("</string>", versionValueStart);
                    if (versionValueStart != -1 && versionValueEnd != -1)
                    {
                        AppVersion = plistContent.Substring(versionValueStart, versionValueEnd - versionValueStart);
                        return;
                    }
                }
            }
            catch
            {
                continue;
            }
        }
        
        // 如果所有路径都失败，设置为开发版本号
        AppVersion = "Dev";
#else
        AppVersion = "Unknown";
#endif
    }

    partial void OnFocusBorderColorChanged(Color value) => Config.Overlays.FocusBorderColor = value;

    partial void OnFocusBorderThicknessChanged(int value) => Config.Overlays.FocusBorderThickness = value;
    
    partial void OnSelectedLanguageChanged(LanguageModel value)
    {
        var language = value.Identifier;
        Localizer.Localizer.Instance.LoadLanguage(language);
        Config.Settings.Language = language;
    }

    public async Task ChangeDir(string parmName)
    {
        var oldValue = SaveDirectory;
        var task = parmName switch
        {
            "SaveDirectory" => this.ChangeDirectory(value => SaveDirectory = value,
                Localizer.Localizer.Instance["SelectDirectory"]),
            _ => Task.CompletedTask
        };
        await task;
        // save the config
        if (oldValue != SaveDirectory)
        {
            Config.Settings.SaveDirectory = SaveDirectory;
            NotifyHelper.Notify(this, Localizer.Localizer.Instance["ConfigChanged"],
                SaveDirectory);
        }
    }
}