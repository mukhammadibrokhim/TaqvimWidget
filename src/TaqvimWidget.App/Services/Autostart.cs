using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TaqvimWidget.Shared;

namespace TaqvimWidget.App.Services;

internal static class Autostart
{
    public static bool Enabled => File.Exists(AppIdentity.StartupShortcut);

    public static void Set(bool enabled)
    {
        try
        {
            if (enabled) ShellLink.Create(AppIdentity.StartupShortcut, Application.ExecutablePath, AppIdentity.Name);
            else File.Delete(AppIdentity.StartupShortcut);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or COMException)
        {
            Log.Write("avtoyuklash: " + ex.Message);
        }
    }
}
