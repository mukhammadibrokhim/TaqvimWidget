using System;
using System.IO;
using TaqvimWidget.Shared;

namespace TaqvimWidget.App.Services;

internal static class AppPaths
{
    /// <summary>Foydalanuvchi ma'lumotlari papkasi (skrinshot rejimida vaqtinchalik papka).</summary>
    public static string Root { get; set; } = AppIdentity.DataDirectory;

    public static string Settings => Data("config.json");
    public static string Traffic => Data("traffic.json");
    public static string Notes => Data("notes.json");
    public static string ErrorLog => Data("error.log");

    private static string Data(string file) => Path.Combine(Root, file);

    /// <summary>1.x versiya ma'lumotlarini bir marta ko'chiradi (asl nusxa saqlanib qoladi).</summary>
    public static void MigrateLegacyData()
    {
        try
        {
            if (Directory.Exists(AppIdentity.DataDirectory) || !Directory.Exists(AppIdentity.LegacyDataDirectory))
            {
                Directory.CreateDirectory(AppIdentity.DataDirectory);
                return;
            }

            Directory.CreateDirectory(AppIdentity.DataDirectory);
            foreach (string name in new[] { "config.json", "traffic.json", "notes.json" })
            {
                string source = Path.Combine(AppIdentity.LegacyDataDirectory, name);
                if (File.Exists(source)) File.Copy(source, Data(name));
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Log.Write("migratsiya: " + ex.Message);
        }
    }
}

internal static class Log
{
    private const long MaxBytes = 256 * 1024;

    public static void Write(string message)
    {
        try
        {
            Directory.CreateDirectory(AppIdentity.DataDirectory);
            var file = new FileInfo(AppPaths.ErrorLog);
            if (file.Exists && file.Length > MaxBytes) file.Delete();
            File.AppendAllText(AppPaths.ErrorLog, $"{DateTime.Now:s} {message}{Environment.NewLine}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Log yozib bo'lmasa, dastur baribir ishlashda davom etadi
        }
    }
}
