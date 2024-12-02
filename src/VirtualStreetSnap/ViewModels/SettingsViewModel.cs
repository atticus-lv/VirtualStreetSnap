using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Services;

namespace VirtualStreetSnap.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{

    [ObservableProperty]
    private AppConfig _config = ConfigService.Instance;


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
    }

    partial void OnSelectedLanguageChanged(LanguageModel value)
    {
        var language = value.Identifier;
        Localizer.Localizer.Instance.LoadLanguage(language);
        Config.Settings.Language = language;
    }

    public async Task ChangeDir(string parmName)
    {
        var task = parmName switch
        {
            "SaveDirectory" => this.ChangeDirectory(value => SaveDirectory = value,
                Localizer.Localizer.Instance["SelectDirectory"]),
            _ => Task.CompletedTask
        };
        await task;
        // save the config
        Config.Settings.SaveDirectory = SaveDirectory;
        NotifyHelper.Notify(this, Localizer.Localizer.Instance["ConfigChanged"],
            SaveDirectory);
    }
}