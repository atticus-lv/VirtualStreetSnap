using System;
using System.Collections.Generic;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace VirtualStreetSnap.Services;

public static class NotifyHelper
{
    private static Dictionary<Window, WindowNotificationManager> NotificationManagers { get; set; } = new();

    public static void Notify(object? context, string? title, string? message, int timeout = 2,
        NotificationType type = NotificationType.Information)
    {
        ArgumentNullException.ThrowIfNull(context);

        // lookup the TopLevel for the context
        var topLevel = ToplevelService.GetTopLevelForContext(context) as Window;
        // check if the TopLevel is registered
        if (topLevel == null) return;
        if (!NotificationManagers.ContainsKey(topLevel)) InitializeNotificationManager(topLevel);
        // set timeout for the notification
        var span = TimeSpan.FromSeconds(timeout);
        NotificationManagers[topLevel].Show(new Notification(title, message, expiration: span, type: type));
    }

    private static void InitializeNotificationManager(Window topLevel)
    {
        var manager = new WindowNotificationManager(topLevel)
        {
            Position = NotificationPosition.BottomCenter,
            MaxItems = 3,
        };
        NotificationManagers.Add(topLevel, manager);
    }
}