using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using VirtualStreetSnap.ViewModels;
using VirtualStreetSnap.Views;

namespace VirtualStreetSnap;

public class ViewLocator : IDataTemplate
{
    // AOT compiled code
    private static readonly Dictionary<Type, Func<Control>> ViewModelViewMap = new()
    {
        { typeof(ImageGalleryViewModel), () => new ImageGalleryView() },
        { typeof(ImageViewerViewModel), () => new ImageViewerView() },
        // Add other mappings as needed
    };

    public Control? Build(object? data)
    {
        if (data is null) return null;
        
        var viewModelType = data.GetType();
        if (!ViewModelViewMap.TryGetValue(viewModelType, out var viewFactory))
            return new TextBlock { Text = "Not Found: " + viewModelType.FullName };
        
        var control = viewFactory();
        control.DataContext = data;
        return control;
    }

    // JIT compiled code
    // public Control? Build(object? data)
    // {
    //     if (data is null)
    //         return null;
    //
    //     var name = data.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
    //     var type = Type.GetType(name);
    //
    //     if (type != null)
    //     {
    //         var control = (Control)Activator.CreateInstance(type)!;
    //         control.DataContext = data;
    //         return control;
    //     }
    //
    //     return new TextBlock { Text = "Not Found: " + name };
    // }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}