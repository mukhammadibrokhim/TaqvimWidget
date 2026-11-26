using System;
using System.Collections.Generic;
using System.Linq;
using TaqvimWidget.Core.Formatting;
using TaqvimWidget.Core.Prayer;
using TaqvimWidget.Core.Storage;

namespace TaqvimWidget.Core.Settings;

/// <summary>config.json tarkibi. Xossa nomlari eski versiya fayli bilan mos (katta-kichik harf farqlanmaydi).</summary>
public sealed class AppSettings
{
    public string Unit { get; set; } = "mbit";
    public bool Overlay { get; set; } = true;
    public bool Prayer { get; set; } = true;
    public string City { get; set; } = "Toshkent";
    public double Fajr { get; set; } = 15.5;
    public double Isha { get; set; } = 15.5;
    public int Asr { get; set; } = 2;
    public int ShomMin { get; set; } = 4;
    public bool PrayerNotify { get; set; } = true;
    public int PreMin { get; set; } = 20;
    public bool AtTime { get; set; } = true;
    public string NotifyList { get; set; } = "Peshin,Asr,Shom";
    public double LimitGB { get; set; }
    public bool System { get; set; } = true;
    public bool Timer { get; set; } = true;
    public bool Holidays { get; set; } = true;
    public bool PingNotify { get; set; } = true;
    public int FocusMin { get; set; } = 25;
    public int BreakMin { get; set; } = 5;
    public SpeedTestRecord? LastSpeedTest { get; set; }

    public SpeedUnit SpeedUnit() => Unit == "mbyte" ? Formatting.SpeedUnit.Megabyte : Formatting.SpeedUnit.Megabit;

    public PrayerParameters PrayerParameters() => new()
    {
        FajrAngle = Fajr,
        IshaAngle = Isha,
        AsrShadowFactor = Asr,
        MaghribOffsetMinutes = ShomMin,
    };

    public List<PrayerName> NotifyPrayers() =>
        (NotifyList ?? string.Empty)
            .Split(',')
            .Select(s => Enum.TryParse(s.Trim(), out PrayerName n) ? (PrayerName?)n : null)
            .Where(n => n is not null and not PrayerName.Quyosh)
            .Select(n => n!.Value)
            .Distinct()
            .ToList();

    /// <summary>Qo'lda tahrirlangan yoki buzilgan qiymatlarni ruxsat etilgan chegaraga qaytaradi.</summary>
    public AppSettings Normalize()
    {
        City = Cities.Find(City).Name;
        Unit = Unit == "mbyte" ? "mbyte" : "mbit";
        Fajr = Clamp(Fajr, 10, 20);
        Isha = Clamp(Isha, 10, 20);
        Asr = Asr == 1 ? 1 : 2;
        ShomMin = (int)Clamp(ShomMin, 0, 30);
        PreMin = (int)Clamp(PreMin, 0, 120);
        LimitGB = Clamp(LimitGB, 0, 100000);
        FocusMin = (int)Clamp(FocusMin, 1, 180);
        BreakMin = (int)Clamp(BreakMin, 1, 60);
        NotifyList ??= string.Empty;
        return this;
    }

    public static AppSettings Load(string path, Action<string>? log = null) =>
        (JsonFile.Read<AppSettings>(path, log) ?? new AppSettings()).Normalize();

    public void Save(string path, Action<string>? log = null) => JsonFile.Write(path, this, log);

    private static double Clamp(double value, double min, double max) =>
        double.IsNaN(value) ? min : Math.Max(min, Math.Min(max, value));
}

public sealed class SpeedTestRecord
{
    public double Download { get; set; }
    public double Upload { get; set; }

    /// <summary>Mahalliy vaqt, ISO 8601 matni (JSON serializer vaqt mintaqasini buzmasligi uchun).</summary>
    public string Time { get; set; } = string.Empty;
}
