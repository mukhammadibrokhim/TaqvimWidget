using System;
using System.Collections.Generic;

namespace TaqvimWidget.Core.Prayer;

/// <summary>Tanlangan shahar uchun namoz vaqtlari; kunlik natijalar keshlanadi.</summary>
public sealed class PrayerSchedule
{
    private readonly Dictionary<DateTime, DayPrayers> _cache = new();
    private City _city;
    private PrayerParameters _parameters;

    public PrayerSchedule(City city, PrayerParameters parameters)
    {
        _city = city;
        _parameters = parameters;
    }

    public City City => _city;

    public void Configure(City city, PrayerParameters parameters)
    {
        _city = city;
        _parameters = parameters;
        _cache.Clear();
    }

    public DayPrayers For(DateTime date)
    {
        var day = date.Date;
        if (!_cache.TryGetValue(day, out var result))
        {
            if (_cache.Count > 8) _cache.Clear();
            result = PrayerCalculator.Calculate(day, _city.Latitude, _city.Longitude, Cities.UtcOffsetHours, _parameters);
            _cache[day] = result;
        }

        return result;
    }

    /// <summary>Hozirdan keyingi birinchi vaqt (kerak bo'lsa ertangi Bomdod).</summary>
    public (PrayerName Name, DateTime Time) Next(DateTime now)
    {
        var today = For(now);
        for (var name = PrayerName.Bomdod; name <= PrayerName.Xufton; name++)
        {
            if (today[name] > now) return (name, today[name]);
        }

        return (PrayerName.Bomdod, For(now.AddDays(1))[PrayerName.Bomdod]);
    }

    /// <summary>
    /// Hozir davom etayotgan namoz vaqti. Quyosh chiqqandan Peshingacha namoz vaqti yo'q (null).
    /// </summary>
    public (PrayerName Name, PrayerName Next, DateTime End)? Current(DateTime now)
    {
        var today = For(now);
        if (now < today[PrayerName.Bomdod]) return (PrayerName.Xufton, PrayerName.Bomdod, today[PrayerName.Bomdod]);

        var current = PrayerName.Bomdod;
        for (var name = PrayerName.Bomdod; name <= PrayerName.Xufton; name++)
        {
            if (today[name] <= now) current = name;
        }

        if (current == PrayerName.Quyosh) return null;
        if (current == PrayerName.Xufton) return (current, PrayerName.Bomdod, For(now.AddDays(1))[PrayerName.Bomdod]);
        return (current, current + 1, today[current + 1]);
    }
}
