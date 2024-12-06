using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        // Set the default values if the config is not existing
        ConfigService.SaveConfig();
        GetFileVersion();
    }
    
    public void GetFileVersion()
    {
        var exeDir = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
        // get all the exe files in the directory
        var exePath = exeDir != null ? Directory.GetFiles(exeDir, "*.exe").FirstOrDefault() : null;
        if (exePath == null) return;
        var version = FileVersionInfo.GetVersionInfo(exePath).FileVersion;
        if (version == null) return;
        AppVersion = version;
    }
    
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