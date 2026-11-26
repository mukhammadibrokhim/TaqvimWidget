using System;
using System.Globalization;

namespace TaqvimWidget.Core.Formatting;

public enum SpeedUnit
{
    Megabit,
    Megabyte,
}

public static class Format
{
    private const double KB = 1024;
    private const double MB = KB * 1024;
    private const double GB = MB * 1024;

    /// <summary>Tezlikni (bayt/soniya) qiymat va birlikka ajratadi.</summary>
    public static (string Value, string Unit) Speed(double bytesPerSecond, SpeedUnit unit)
    {
        if (unit == SpeedUnit.Megabyte)
        {
            return bytesPerSecond >= MB
                ? ((bytesPerSecond / MB).ToString("N1"), "MB/s")
                : ((bytesPerSecond / KB).ToString("N0"), "KB/s");
        }

        double megabit = bytesPerSecond * 8 / 1e6;
        return megabit >= 1
            ? (megabit.ToString("N1"), "Mbit/s")
            : ((bytesPerSecond * 8 / 1e3).ToString("N0"), "Kbit/s");
    }

    /// <summary>Ikkinchi (qo'shimcha) birlikdagi tezlik.</summary>
    public static string AlternativeSpeed(double bytesPerSecond, SpeedUnit unit) =>
        unit == SpeedUnit.Megabyte
            ? $"{bytesPerSecond * 8 / 1e6:N1} Mbit/s"
            : $"{bytesPerSecond / MB:N2} MB/s";

    public static string Bytes(double bytes)
    {
        if (bytes >= GB) return $"{bytes / GB:N2} GB";
        if (bytes >= MB) return $"{bytes / MB:N0} MB";
        return $"{bytes / KB:N0} KB";
    }

    public static string Remaining(TimeSpan span)
    {
        int minutes = (int)Math.Ceiling(span.TotalMinutes);
        return minutes >= 60 ? $"{minutes / 60} soat {minutes % 60} daq" : $"{minutes} daq";
    }

    public static string MinutesSeconds(int seconds) => $"{seconds / 60:00}:{seconds % 60:00}";

    public static string Clock(DateTime time) => time.ToString("HH:mm", CultureInfo.InvariantCulture);

    public static string IsoDate(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
