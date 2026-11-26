using System;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace TaqvimWidget.Core.Storage;

/// <summary>JSON faylni o'qish va atomik yozish.</summary>
public static class JsonFile
{
    public static T? Read<T>(string path, Action<string>? log = null)
        where T : class
    {
        try
        {
            if (!File.Exists(path)) return null;
            return new JavaScriptSerializer().Deserialize<T>(File.ReadAllText(path, Encoding.UTF8));
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or InvalidOperationException or FormatException or InvalidCastException or UnauthorizedAccessException)
        {
            log?.Invoke($"{Path.GetFileName(path)} o'qilmadi: {ex.Message}");
            return null;
        }
    }

    public static void Write(string path, object value, Action<string>? log = null)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            string temp = path + ".tmp";
            File.WriteAllText(temp, new JavaScriptSerializer().Serialize(value), new UTF8Encoding(false));

            // Yozish paytida kompyuter o'chib qolsa ham eski fayl buzilmaydi
            if (File.Exists(path)) File.Replace(temp, path, null);
            else File.Move(temp, path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            log?.Invoke($"{Path.GetFileName(path)} yozilmadi: {ex.Message}");
        }
    }
}
