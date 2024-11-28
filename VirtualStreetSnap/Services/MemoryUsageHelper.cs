using System;

namespace VirtualStreetSnap.Services;

public static class MemoryUsageHelper
{
    public static void MeasureMemoryUsage(Action action)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long beforeMemory = GC.GetTotalMemory(true);

        action();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long afterMemory = GC.GetTotalMemory(true);

        Console.WriteLine($"Memory usage: {afterMemory - beforeMemory} bytes");
    }
}