using System;
using System.Collections.Generic;
using System.Globalization;

namespace TaqvimWidget.Core.Calendar;

public sealed class HolidayProvider
{
    private static readonly (int Month, int Day, string Name)[] Fixed =
    {
        (1, 1, "Yangi yil"),
        (3, 8, "Xotin-qizlar kuni"),
        (3, 21, "Navro'z"),
        (5, 9, "Xotira va qadrlash kuni"),
        (9, 1, "Mustaqillik kuni"),
        (10, 1, "O'qituvchi va murabbiylar kuni"),
        (12, 8, "Konstitutsiya kuni"),
    };

    private readonly Dictionary<int, Dictionary<DateTime, string>> _cache = new();

    public bool IsHoliday(DateTime date) => ForYear(date.Year).ContainsKey(date.Date);

    public (DateTime Date, string Name)? Next(DateTime from)
    {
        for (int i = 0; i < 400; i++)
        {
            var day = from.Date.AddDays(i);
            if (ForYear(day.Year).TryGetValue(day, out var name)) return (day, name);
        }

        return null;
    }

    private Dictionary<DateTime, string> ForYear(int year)
    {
        if (_cache.TryGetValue(year, out var map)) return map;

        map = new Dictionary<DateTime, string>();
        foreach (var (month, day, name) in Fixed) map[new DateTime(year, month, day)] = name;
        AddHijriHolidays(map, year);
        _cache[year] = map;
        return map;
    }

    // Hayit kunlari Umm al-Qura taqvimi bo'yicha: taxminiy (yulduzcha bilan belgilanadi)
    private static void AddHijriHolidays(Dictionary<DateTime, string> map, int year)
    {
        var hijri = new UmAlQuraCalendar();
        for (var day = new DateTime(year, 1, 1); day.Year == year; day = day.AddDays(1))
        {
            if (day < hijri.MinSupportedDateTime || day > hijri.MaxSupportedDateTime) return;

            int month = hijri.GetMonth(day);
            int dayOfMonth = hijri.GetDayOfMonth(day);
            if (month == 10 && dayOfMonth == 1) map[day] = "Ramazon hayiti*";
            if (month == 12 && dayOfMonth == 10) map[day] = "Qurbon hayiti*";
        }
    }
}
