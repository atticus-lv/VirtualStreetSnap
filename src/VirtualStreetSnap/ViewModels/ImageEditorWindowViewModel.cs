﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Views;

namespace VirtualStreetSnap.ViewModels;

public partial class ImageEditorWindowViewModel : ViewModelBase
{
    public ObservableCollection<ImageEditorView> Pages { get; } = new();

    [ObservableProperty]
    private ImageEditorView? _currentPage;


    public ImageEditorWindowViewModel()
    {
        if (Design.IsDesignMode)
        {
            Pages.Add(new ImageEditorView()
            {
                DataContext = new DesignImageEditorViewModel()
            });
            Pages.Add(new ImageEditorView()
            {
                DataContext = new DesignImageEditorViewModel()
            });
            Pages.Add(new ImageEditorView()
            {
                DataContext = new DesignImageEditorViewModel()
            });
            CurrentPage = Pages.First();
        }
    }

    public void AddPage(ImageModelBase? image)
    {
        if (image == null) return;
        // if image already exists, set it as current page
        if (Pages.Any(page =>
                page.DataContext is ImageEditorViewModel viewModel && viewModel.EditImageViewer.ViewImage == image))
        {
            CurrentPage = Pages.First(page =>
                page.DataContext is ImageEditorViewModel viewModel && viewModel.EditImageViewer.ViewImage == image);
            return;
        }

        ;
        Pages.Add(new ImageEditorView()
        {
            DataContext = new ImageEditorViewModel(image)
        });
        CurrentPage = Pages.Last();
    }

    public void RemovePage(ImageEditorView page)
    {
        if (!Pages.Contains(page)) return;
        Pages.Remove(page);
        CurrentPage = Pages.LastOrDefault();
    }

    public event EventHandler? ImageSaved;

    private void OnImageSaved()
    {
        ImageSaved?.Invoke(this, EventArgs.Empty);
    }

    public void SaveCurrentPageImage(bool saveAsNew)
    {
        if (CurrentPage is null)return;
        var viewModel = CurrentPage.DataContext as ImageEditorViewModel;
        viewModel?.SaveImageToGalleryDirectory(saveAsNew);
        OnImageSaved();
    }
}