using System;

namespace TaqvimWidget.Core.Prayer;

public enum PrayerName
{
    Bomdod,
    Quyosh,
    Peshin,
    Asr,
    Shom,
    Xufton,
}

public sealed class PrayerParameters
{
    public double FajrAngle { get; set; } = 15.5;
    public double IshaAngle { get; set; } = 15.5;

    /// <summary>Asr soya koeffitsienti: 2 - Hanafiy, 1 - Shofe'iy.</summary>
    public int AsrShadowFactor { get; set; } = 2;

    /// <summary>Shom quyosh botishidan necha daqiqa keyin (islom.uz: 4).</summary>
    public int MaghribOffsetMinutes { get; set; } = 4;
}

public sealed class DayPrayers
{
    private readonly DateTime[] _times;

    public DayPrayers(DateTime date, DateTime[] times)
    {
        Date = date;
        _times = times;
    }

    public DateTime Date { get; }

    public DateTime this[PrayerName name] => _times[(int)name];
}

/// <summary>
/// Quyosh holatiga asoslangan oflayn hisob-kitob (praytimes.org algoritmi).
/// </summary>
public static class PrayerCalculator
{
    private const double Rad = Math.PI / 180;

    // Atmosfera refraksiyasi va quyosh diskining radiusi hisobga olingan balandlik
    private const double SunsetAngle = 0.833;

    public static DayPrayers Calculate(DateTime date, double latitude, double longitude, double utcOffset, PrayerParameters p)
    {
        double jd = JulianDate(date.Year, date.Month, date.Day) - longitude / 360.0;

        double declination = Sun(jd + 13.0 / 24).Declination;
        double asrAngle = -Math.Atan(1.0 / (p.AsrShadowFactor + Math.Tan(Math.Abs(latitude - declination) * Rad))) / Rad;

        var hours = new[]
        {
            AngleTime(jd, latitude, p.FajrAngle, 5.0 / 24, beforeNoon: true),
            AngleTime(jd, latitude, SunsetAngle, 6.0 / 24, beforeNoon: true) + 0.25 / 60.0, // islom.uz bilan soniya darajasidagi farq
            MidDay(jd, 12.0 / 24),
            AngleTime(jd, latitude, asrAngle, 13.0 / 24, beforeNoon: false),
            AngleTime(jd, latitude, SunsetAngle, 18.0 / 24, beforeNoon: false) + p.MaghribOffsetMinutes / 60.0,
            AngleTime(jd, latitude, p.IshaAngle, 18.0 / 24, beforeNoon: false),
        };

        var times = new DateTime[hours.Length];
        for (int i = 0; i < hours.Length; i++)
        {
            double local = hours[i] + utcOffset - longitude / 15.0;
            times[i] = date.Date.AddMinutes(Math.Round(local * 60));
        }

        return new DayPrayers(date.Date, times);
    }

    private static double Wrap(double value, double range) => value - range * Math.Floor(value / range);

    private static double JulianDate(int year, int month, int day)
    {
        if (month <= 2)
        {
            year -= 1;
            month += 12;
        }

        double a = Math.Floor(year / 100.0);
        double b = 2 - a + Math.Floor(a / 4);
        return Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1)) + day + b - 1524.5;
    }

    private static (double Declination, double EquationOfTime) Sun(double jd)
    {
        double d = jd - 2451545.0;
        double g = Wrap(357.529 + 0.98560028 * d, 360);
        double q = Wrap(280.459 + 0.98564736 * d, 360);
        double l = Wrap(q + 1.915 * Math.Sin(g * Rad) + 0.020 * Math.Sin(2 * g * Rad), 360);
        double e = 23.439 - 0.00000036 * d;
        double ra = Wrap(Math.Atan2(Math.Cos(e * Rad) * Math.Sin(l * Rad), Math.Cos(l * Rad)) / Rad / 15, 24);

        double eqt = q / 15 - ra;
        if (eqt > 12) eqt -= 24;
        if (eqt < -12) eqt += 24;

        double declination = Math.Asin(Math.Sin(e * Rad) * Math.Sin(l * Rad)) / Rad;
        return (declination, eqt);
    }

    private static double MidDay(double jd, double dayFraction) => Wrap(12 - Sun(jd + dayFraction).EquationOfTime, 24);

    private static double AngleTime(double jd, double latitude, double angle, double dayFraction, bool beforeNoon)
    {
        double declination = Sun(jd + dayFraction).Declination;
        double noon = MidDay(jd, dayFraction);
        double cos = (-Math.Sin(angle * Rad) - Math.Sin(declination * Rad) * Math.Sin(latitude * Rad))
                     / (Math.Cos(declination * Rad) * Math.Cos(latitude * Rad));
        double offset = Math.Acos(Math.Max(-1.0, Math.Min(1.0, cos))) / Rad / 15;
        return beforeNoon ? noon - offset : noon + offset;
    }
}
