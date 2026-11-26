using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using TaqvimWidget.Shared;

namespace TaqvimWidget.Setup;

/// <summary>Joriy foydalanuvchi uchun o'rnatadi (administrator huquqi kerak emas).</summary>
internal static class Installer
{
    public static string Install(bool autostart, bool desktopShortcut)
    {
        StopRunningInstances();
        RemoveVersion1();

        string directory = AppIdentity.InstallDirectory;
        Directory.CreateDirectory(directory);
        long bytes = Extract(AppIdentity.ExeName, directory) + Extract(AppIdentity.CoreDllName, directory);
        string executable = Path.Combine(directory, AppIdentity.ExeName);

        ShellLink.Create(AppIdentity.StartMenuShortcut, executable, "Internet tezligi, namoz vaqtlari va kalendar", AppIdentity.AppUserModelId);

        if (autostart) ShellLink.Create(AppIdentity.StartupShortcut, executable, AppIdentity.Name);
        else File.Delete(AppIdentity.StartupShortcut);

        if (desktopShortcut) ShellLink.Create(AppIdentity.DesktopShortcut, executable, AppIdentity.Name);

        Register(executable, directory, bytes);
        return executable;
    }

    private static long Extract(string resource, string directory)
    {
        using var source = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource)
                           ?? throw new InvalidOperationException("O'rnatuvchi ichida fayl topilmadi: " + resource);
        using var target = File.Create(Path.Combine(directory, resource));
        source.CopyTo(target);
        return source.Length;
    }

    private static void StopRunningInstances()
    {
        foreach (string name in new[] { "TaqvimWidget", "DesktopWidget" })
        {
            foreach (var process in Process.GetProcessesByName(name))
            {
                try
                {
                    process.Kill();
                    process.WaitForExit(3000);
                }
                catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
                {
                    // Jarayon allaqachon tugagan; fayl band bo'lsa, keyingi qadam aniq xato beradi
                }
            }
        }
    }

    // Dastlabki "Desktop Widget" nomli versiyani tozalaydi; foydalanuvchi ma'lumotlariga tegilmaydi
    private static void RemoveVersion1()
    {
        foreach (var (folder, file) in new[]
        {
            (Environment.SpecialFolder.Startup, "DesktopWidget.lnk"),
            (Environment.SpecialFolder.Programs, "Desktop Widget.lnk"),
            (Environment.SpecialFolder.DesktopDirectory, "Desktop Widget.lnk"),
        })
        {
            File.Delete(Path.Combine(Environment.GetFolderPath(folder), file));
        }

        Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\DesktopWidget", false);
    }

    private static void Register(string executable, string directory, long bytes)
    {
        using var key = Registry.CurrentUser.CreateSubKey(AppIdentity.UninstallKey);
        key.SetValue("DisplayName", AppIdentity.Name);
        key.SetValue("DisplayVersion", Assembly.GetExecutingAssembly().GetName().Version.ToString(3));
        key.SetValue("Publisher", AppIdentity.Name);
        key.SetValue("InstallLocation", directory);
        key.SetValue("DisplayIcon", executable + ",0");
        key.SetValue("UninstallString", $"\"{executable}\" /uninstall");
        key.SetValue("NoModify", 1, RegistryValueKind.DWord);
        key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
        key.SetValue("EstimatedSize", (int)(bytes / 1024), RegistryValueKind.DWord);
    }
}
