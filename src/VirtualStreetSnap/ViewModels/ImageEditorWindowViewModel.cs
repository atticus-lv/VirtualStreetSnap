﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using VirtualStreetSnap.Models;
using VirtualStreetSnap.Services;
using VirtualStreetSnap.Views;

namespace VirtualStreetSnap.ViewModels;

public partial class ImageEditorWindowViewModel : ViewModelBase
{
    public ObservableCollection<ImageEditorView> Pages { get; } = new();

    [ObservableProperty]
    private ImageEditorView? _currentPage;

    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _loadingMessage = "Loading";

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

    public async void AddPage(ImageModelBase? image)
    {
        if (image == null) return;

        // Reload the image to ensure the Bitmap is not null
        IsLoading = true;
        LoadingMessage = "LoadingImage";
        await image.LoadImageAsync();
        IsLoading = false;

        // if image already exists, set it as current page
        if (Pages.Any(page =>
                page.DataContext is ImageEditorViewModel viewModel &&
                viewModel.EditImageViewer.ViewImage?.ImgPath == image.ImgPath))
        {
            CurrentPage = Pages.First(page =>
                page.DataContext is ImageEditorViewModel viewModel &&
                viewModel.EditImageViewer.ViewImage?.ImgPath == image.ImgPath);
            return;
        }

        Console.WriteLine($"Add page for {image.ImgPath}");

        try
        {   
            IsLoading = true;
            LoadingMessage = "LoadingView";
            var viewmodel = await Task.Run(() => new ImageEditorViewModel(image));
            IsLoading = false;

            Pages.Add(new ImageEditorView()
            {
                DataContext = viewmodel
            });
            CurrentPage = Pages.Last();
            var viewModel = CurrentPage.DataContext as ImageEditorViewModel;
            viewModel.IsDirty = false;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async void RemovePage(ImageEditorView page)
    {
        if (!Pages.Contains(page)) return;

        void RemoveAction()
        {
            Pages.Remove(page);
            CurrentPage = Pages.LastOrDefault();
        }

        if (Design.IsDesignMode)
        {
            RemoveAction();
            return;
        }

        if (page.DataContext is ImageEditorViewModel { IsDirty: false })
        {
            RemoveAction();
            return;
        }

        var dialog = new ConfirmDialog()
        {
            DataContext = new ConfirmDialogViewModel()
            {
                Title = "UnsavedChanges",
                Message = "AreYouSureToClose?",
                Width = 300,
                Height = 150,
            }
        };
        var topLevel = ToplevelService.GetTopLevelForContext(this) as Window;
        var result = await dialog.ShowDialog<bool>(topLevel);
        if (result)
        {
            RemoveAction();
        }
    }

    public event EventHandler? ImageSaved;

    private void OnImageSaved()
    {
        ImageSaved?.Invoke(this, EventArgs.Empty);
    }

    public async void OpenImageAndAddPage()
    {
        if (Design.IsDesignMode) return;
        var selectedFile = await this.SelectFile("Select");
        if (string.IsNullOrEmpty(selectedFile)) return;
        // check it ends with a valid image extension
        var ext = System.IO.Path.GetExtension(selectedFile).ToLower();
        List<string> filterList = [".jpg", ".jpeg", ".png", ".bmp"];
        if (!filterList.Contains(ext))
        {
            NotifyHelper.Notify(this,
                Localizer.Localizer.Instance["Warning"],
                Localizer.Localizer.Instance["InvalidFileFormat"],
                3, NotificationType.Error);
            return;
        }

        ;
        var image = new ImageModelBase(selectedFile);
        AddPage(image);
    }

    public async void SaveCurrentPageImage(bool saveAsNew)
    {
        if (CurrentPage is null) return;
        var viewModel = CurrentPage.DataContext as ImageEditorViewModel;
        if (saveAsNew)
        {
            viewModel?.SaveImageToGalleryDirectory(saveAsNew);
            OnImageSaved();
        }
        else
        {
            var dialog = new ConfirmDialog()
            {
                DataContext = new ConfirmDialogViewModel()
                {
                    Title = "Warning",
                    Message = "ThePictureIsAboutToBeOverwritten",
                    Width = 300,
                    Height = 150,
                    ConfirmButtonText = "Overwrite",
                }
            };
            var topLevel = ToplevelService.GetTopLevelForContext(this) as Window;
            var result = await dialog.ShowDialog<bool>(topLevel);
            if (!result) return;
            viewModel?.SaveImageToGalleryDirectory(saveAsNew);
            OnImageSaved();
        }
    }
}