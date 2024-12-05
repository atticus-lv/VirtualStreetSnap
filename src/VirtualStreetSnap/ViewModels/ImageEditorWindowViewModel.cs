using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
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
        await image.LoadImageAsync();

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
            Pages.Add(new ImageEditorView()
            {
                DataContext = new ImageEditorViewModel(image)
            });

            CurrentPage = Pages.Last();
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
                Title = "Unsaved Changes",
                Message = "Are you sure to close?",
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

    public void SaveCurrentPageImage(bool saveAsNew)
    {
        if (CurrentPage is null) return;
        var viewModel = CurrentPage.DataContext as ImageEditorViewModel;
        Console.WriteLine($"Save image for {viewModel?.EditImageViewer.ViewImage?.ImgPath}");
        viewModel?.SaveImageToGalleryDirectory(saveAsNew);
        OnImageSaved();
    }
}