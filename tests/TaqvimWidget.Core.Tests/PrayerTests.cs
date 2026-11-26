using System;
using System.Globalization;
using System.Linq;
using TaqvimWidget.Core.Prayer;
using Xunit;

namespace TaqvimWidget.Core.Tests;

public class PrayerTests
{
    private static PrayerSchedule Tashkent() => new(Cities.Find("Toshkent"), new PrayerParameters());

    private static DateTime At(string value) => DateTime.Parse(value, CultureInfo.InvariantCulture);

    // Etalon qiymatlar loyihaning oldingi (tekshirilgan) versiyasidan olingan
    [Theory]
    [InlineData("2026-01-15", "Toshkent", "06:23 07:47 12:32 15:37 17:22 18:42")]
    [InlineData("2026-03-21", "Toshkent", "05:06 06:26 12:30 16:47 18:40 19:55")]
    [InlineData("2026-06-21", "Samarqand", "03:24 05:05 12:34 17:46 20:07 21:44")]
    [InlineData("2026-09-01", "Nukus", "05:02 06:27 13:02 17:40 19:40 21:01")]
    [InlineData("2026-11-26", "Toshkent", "06:00 07:24 12:10 15:16 17:01 18:20")]
    [InlineData("2026-12-31", "Andijon", "06:10 07:35 12:14 15:12 16:57 18:17")]
    public void Calculate_matches_reference_times(string date, string city, string expected)
    {
        var day = new PrayerSchedule(Cities.Find(city), new PrayerParameters()).For(At(date));

        var actual = Enum.GetValues(typeof(PrayerName)).Cast<PrayerName>().Select(n => day[n].ToString("HH:mm"));

        Assert.Equal(expected, string.Join(" ", actual));
    }

    [Fact]
    public void Current_is_null_between_sunrise_and_noon() => Assert.Null(Tashkent().Current(At("2026-11-26 09:00")));

    [Fact]
    public void Current_before_fajr_is_isha()
    {
        var current = Tashkent().Current(At("2026-11-26 02:00"));

        Assert.Equal(PrayerName.Xufton, current!.Value.Name);
        Assert.Equal(PrayerName.Bomdod, current.Value.Next);
    }

    [Fact]
    public void Next_after_isha_is_tomorrows_fajr()
    {
        var next = Tashkent().Next(At("2026-11-26 23:00"));

        Assert.Equal(PrayerName.Bomdod, next.Name);
        Assert.Equal(27, next.Time.Day);
    }

    [Fact]
    public void Planner_sends_each_notification_once()
    {
        var planner = new PrayerNotificationPlanner();
        var schedule = Tashkent();
        var enabled = new[] { PrayerName.Peshin };

        Assert.Empty(planner.Due(At("2026-11-26 11:49"), schedule, enabled, 20, true));
        var before = Assert.Single(planner.Due(At("2026-11-26 11:50"), schedule, enabled, 20, true));
        Assert.Equal("Peshin namoziga 20 daqiqa qoldi", before.Title);
        Assert.Empty(planner.Due(At("2026-11-26 11:51"), schedule, enabled, 20, true));
        Assert.Equal("Peshin vaqti kirdi", Assert.Single(planner.Due(At("2026-11-26 12:10"), schedule, enabled, 20, true)).Title);
        Assert.Empty(planner.Due(At("2026-11-26 12:11"), schedule, enabled, 20, true));
    }

    [Fact]
    public void Planner_skips_stale_notifications() =>
        Assert.Empty(new PrayerNotificationPlanner().Due(At("2026-11-26 13:00"), Tashkent(), new[] { PrayerName.Peshin }, 20, true));
}
