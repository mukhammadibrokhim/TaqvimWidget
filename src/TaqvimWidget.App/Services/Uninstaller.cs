using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using TaqvimWidget.Shared;

namespace TaqvimWidget.App.Services;

internal static class Uninstaller
{
    public static void Run()
    {
        if (!Ask("Taqvim Widget o'chirilsinmi?")) return;

        using var current = Process.GetCurrentProcess();
        foreach (var process in Process.GetProcessesByName("TaqvimWidget"))
        {
            if (process.Id != current.Id) TryDo(() => { process.Kill(); process.WaitForExit(3000); });
        }

        TryDo(() => File.Delete(AppIdentity.StartupShortcut));
        TryDo(() => File.Delete(AppIdentity.StartMenuShortcut));
        TryDo(() => File.Delete(AppIdentity.DesktopShortcut));
        TryDo(() => Registry.CurrentUser.DeleteSubKeyTree(AppIdentity.UninstallKey, false));
        TryDo(() => Registry.CurrentUser.DeleteSubKeyTree(AppIdentity.ToastKey, false));

        if (Ask("Sozlamalar, eslatmalar va trafik hisobi ham o'chirilsinmi?"))
        {
            TryDo(() => Directory.Delete(AppIdentity.DataDirectory, true));
        }

        RemoveInstallDirectoryAfterExit(current.MainModule.FileName);
        MessageBox.Show("Taqvim Widget o'chirildi.", AppIdentity.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // Ishlab turgan fayl o'zini o'chira olmaydi. Faqat o'rnatish papkasi va faqat o'z fayllari o'chiriladi.
    private static void RemoveInstallDirectoryAfterExit(string executablePath)
    {
        string directory = AppIdentity.InstallDirectory;
        if (!executablePath.StartsWith(directory + "\\", StringComparison.OrdinalIgnoreCase)) return;

        string command = $"/c ping 127.0.0.1 -n 3 >nul & del /q \"{Path.Combine(directory, AppIdentity.ExeName)}\" " +
                         $"\"{Path.Combine(directory, AppIdentity.CoreDllName)}\" & rmdir \"{directory}\"";
        TryDo(() => Process.Start(new ProcessStartInfo("cmd.exe", command)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
        }));
    }

    private static bool Ask(string question) =>
        MessageBox.Show(question, AppIdentity.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    private static void TryDo(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            Log.Write("o'chirish: " + ex.Message);
        }
    }
}
