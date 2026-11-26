using System;
using System.Globalization;

namespace TaqvimWidget.Core.Calendar;

/// <summary>Dushanbadan boshlanadigan 6x7 oylik to'r.</summary>
public static class MonthGrid
{
    public const int Rows = 6;
    public const int Columns = 7;

    private static readonly string[] MonthNames =
    {
        "Yanvar", "Fevral", "Mart", "Aprel", "May", "Iyun",
        "Iyul", "Avgust", "Sentabr", "Oktyabr", "Noyabr", "Dekabr",
    };

    public static string MonthName(int month) => MonthNames[month - 1];

    /// <summary>To'rning birinchi (chap yuqori) katagidagi sana.</summary>
    public static DateTime FirstCell(int year, int month)
    {
        var first = new DateTime(year, month, 1);
        return first.AddDays(-(((int)first.DayOfWeek + 6) % 7));
    }

    public static DateTime Cell(int year, int month, int index) => FirstCell(year, month).AddDays(index);

    /// <summary>ISO 8601 hafta raqami.</summary>
    public static int IsoWeek(DateTime date)
    {
        // Hafta raqami shu haftaning payshanbasi bo'yicha aniqlanadi
        var thursday = date.Date.AddDays(3 - ((int)date.DayOfWeek + 6) % 7);
        return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(thursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }
}
