using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace VirtualStreetSnap.Services;

/// <summary>
///     A helper class to manage dialogs via extension methods. Add more on your own
/// </summary>
public static class DialogHelper
{
    /// <summary>
    ///     Shows an open file dialog for a registered context, most likely a ViewModel
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="title">The dialog title or a default is null</param>
    /// <param name="selectMany">Is selecting many files allowed?</param>
    /// <returns>An array of file names</returns>
    /// <exception cref="ArgumentNullException">if context was null</exception>
    public static async Task<IEnumerable<string>?> OpenFileDialogAsync(this object? context, string? title = null,
        bool selectMany = true, bool selectFolder = false)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        // lookup the TopLevel for the context
        var topLevel = ToplevelService.GetTopLevelForContext(context);

        if (topLevel == null) return null;
        
        if (!selectFolder)
        {
            var storageFiles = await topLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    AllowMultiple = selectMany,
                    Title = title ?? "Select any file(s)"
                });
            return storageFiles.Count == 0 ? null :
                // convert the StorageFile to a string without file:/// prefix
                storageFiles.Select(s => s.Path.ToString().Remove(0, 8));
        }
        else
        {
            var storageFiles = await topLevel.StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions
                {
                    AllowMultiple = selectMany,
                    Title = title ?? "Select a folder"
                });
            if (storageFiles.Count == 0) return null;
            return storageFiles.Select(s => s.Path.ToString().Remove(0, 8));
        }

    }

    public delegate void SetSelectedPath(string path);

    public static async Task ChangeDirectory(this object? context, SetSelectedPath setDirectory,
        string title = "Select")
    {   
        var selectedFiles = await context.OpenFileDialogAsync(title, false, true);
        if (selectedFiles is null)return;
        setDirectory(selectedFiles.ElementAt(0));
    }
}