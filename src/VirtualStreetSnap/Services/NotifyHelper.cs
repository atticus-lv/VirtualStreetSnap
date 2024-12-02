using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace VirtualStreetSnap.Services;

public static class NotifyHelper
{
    private static Dictionary<Window, WindowNotificationManager> NotificationManagers { get;set; } = new();
    
    public static void Notify(object? context, string? title, string? message)
    {
        ArgumentNullException.ThrowIfNull(context);

        // lookup the TopLevel for the context
        var topLevel = ToplevelService.GetTopLevelForContext(context) as Window;
        // check if the TopLevel is registered
        if (topLevel == null) return;
        if (!NotificationManagers.ContainsKey(topLevel))InitializeNotificationManager(topLevel);
        NotificationManagers[topLevel].Show(new Notification(title, message));
    }
    private static void InitializeNotificationManager(Window topLevel)
    {
        var manager = new WindowNotificationManager(topLevel)
        {
            Position = NotificationPosition.BottomRight,
            MaxItems = 3
        };
        NotificationManagers.Add(topLevel, manager);
    }
}