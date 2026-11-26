using System;
using System.IO;

namespace TaqvimWidget.Shared;

/// <summary>Dastur va o'rnatuvchi uchun umumiy nomlar va yo'llar.</summary>
internal static class AppIdentity
{
    public const string Name = "Taqvim Widget";
    public const string ExeName = "TaqvimWidget.exe";
    public const string CoreDllName = "TaqvimWidget.Core.dll";
    public const string AppUserModelId = "TaqvimWidget";
    public const string UninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\TaqvimWidget";
    public const string ToastKey = @"Software\Classes\AppUserModelId\" + AppUserModelId;

    public static string InstallDirectory => Path.Combine(Folder(Environment.SpecialFolder.LocalApplicationData), @"Programs\TaqvimWidget");
    public static string DataDirectory => Path.Combine(Folder(Environment.SpecialFolder.ApplicationData), "TaqvimWidget");

    /// <summary>1.x versiyalar ishlatgan ma'lumotlar papkasi.</summary>
    public static string LegacyDataDirectory => Path.Combine(Folder(Environment.SpecialFolder.ApplicationData), "DesktopWidget");

    public static string StartMenuShortcut => Path.Combine(Folder(Environment.SpecialFolder.Programs), Name + ".lnk");
    public static string DesktopShortcut => Path.Combine(Folder(Environment.SpecialFolder.DesktopDirectory), Name + ".lnk");
    public static string StartupShortcut => Path.Combine(Folder(Environment.SpecialFolder.Startup), "TaqvimWidget.lnk");

    private static string Folder(Environment.SpecialFolder folder) => Environment.GetFolderPath(folder);
}
