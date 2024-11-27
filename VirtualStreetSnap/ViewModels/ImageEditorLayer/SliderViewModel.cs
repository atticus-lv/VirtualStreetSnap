using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace VirtualStreetSnap.ViewModels.ImageEditorLayer;

public class SliderViewModel : ViewModelBase
{
    public string Name { get; set; }
    public float MinValue { get; set; }
    public float MaxValue { get; set; }
    public float DefaultValue { get; set; }
    public Action<float> OnChange { get; set; }

    private float _currentValue;

    public float CurrentValue
    {
        get => _currentValue;
        set
        {
            if (SetProperty(ref _currentValue, value)) OnChange?.Invoke(value);
        }
    }

    public ICommand ResetCommand { get; }

    public SliderViewModel(float defaultValue)
    {
        DefaultValue = defaultValue;
        _currentValue = defaultValue;
        ResetCommand = new RelayCommand(() => CurrentValue = DefaultValue);
    }
}