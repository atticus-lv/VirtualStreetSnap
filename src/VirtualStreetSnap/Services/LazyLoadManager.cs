using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using VirtualStreetSnap.ViewModels;

namespace VirtualStreetSnap.Services;

public class LazyLoadManager
{
    private const int BatchSize = 20;
    private Dictionary<string, DateTime> _allImagePaths = new();
    private int _currentBatchIndex;
    
    public bool IsInitialized { get; private set; }
    public ObservableCollection<ImageModelBase> Thumbnails { get; } = new();

    public void Initialize(string saveDirectory)
    {
        if (!Directory.Exists(saveDirectory)) return;

        _allImagePaths = Directory.GetFiles(saveDirectory, "*.png")
            .ToDictionary(file => file, File.GetLastWriteTime);
        _allImagePaths = _allImagePaths
            .OrderByDescending(kv => kv.Value)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
            
        Thumbnails.Clear();
        _currentBatchIndex = 0;
        LoadNextBatch();
        IsInitialized = true;
    }

    public void LoadNextBatch()
    {
        var nextBatch = _allImagePaths.Skip(_currentBatchIndex * BatchSize).Take(BatchSize);
        foreach (var kv in nextBatch)
        {
            Thumbnails.Add(new ImageModelBase(kv.Key));
        }
        _currentBatchIndex++;
    }
} 