using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using Microsoft.Win32;
using TaqvimWidget.App.Interop;
using TaqvimWidget.Core;
using TaqvimWidget.Shared;

namespace TaqvimWidget.App.Services;

/// <summary>Windows bildirishnomasi; ishlamasa tray balloniga qaytadi.</summary>
internal sealed class Notifier
{
    private readonly NotifyIcon _tray;

    public Notifier(NotifyIcon tray) => _tray = tray;

    /// <summary>
    /// Windows bildirishnomani faqat ro'yxatdan o'tgan va Start menyuda AppUserModelID li yorlig'i bor
    /// dasturdan ko'rsatadi.
    /// </summary>
    public static void Register(string executablePath)
    {
        try
        {
            using (var key = Registry.CurrentUser.CreateSubKey(AppIdentity.ToastKey))
            {
                key.SetValue("DisplayName", AppIdentity.Name);
            }

            // Faqat o'rnatilgan nusxa yorliqni yangilaydi, ishlab chiqishdagi nusxa unga tegmaydi
            if (executablePath.StartsWith(AppIdentity.InstallDirectory, StringComparison.OrdinalIgnoreCase))
            {
                ShellLink.Create(AppIdentity.StartMenuShortcut, executablePath, AppIdentity.Name, AppIdentity.AppUserModelId);
            }
        }
        catch (Exception ex) when (ex is COMException or UnauthorizedAccessException or SecurityException or System.IO.IOException)
        {
            Log.Write("bildirishnoma ro'yxati: " + ex.Message);
        }
    }

    public void Show(Notification notification)
    {
        try
        {
            WinRtToast.Show(AppIdentity.AppUserModelId, notification.Title, notification.Message);
        }
        catch (Exception ex) when (ex is COMException or InvalidCastException or TypeLoadException or EntryPointNotFoundException or ArgumentException)
        {
            Log.Write("toast: " + ex.Message);
            _tray.ShowBalloonTip(15000, notification.Title, notification.Message, ToolTipIcon.Info);
        }
    }
}
