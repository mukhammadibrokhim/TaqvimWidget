using System;
using System.IO;
using TaqvimWidget.Core.Calendar;
using TaqvimWidget.Core.Formatting;
using TaqvimWidget.Core.Network;
using TaqvimWidget.Core.Notes;
using TaqvimWidget.Core.Pomodoro;
using TaqvimWidget.Core.Prayer;
using TaqvimWidget.Core.Settings;
using Xunit;

namespace TaqvimWidget.Core.Tests;

public sealed class CoreTests : IDisposable
{
    private static readonly DateTime Noon = new(2026, 11, 26, 12, 0, 0);
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "taqvim-tests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
    }

    private string TempFile(string name, string? content = null)
    {
        Directory.CreateDirectory(_directory);
        string path = Path.Combine(_directory, name);
        if (content is not null) File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void SpeedMeter_computes_bytes_per_second_and_ignores_counter_reset()
    {
        var meter = new SpeedMeter();
        meter.Sample(1000, 500, 1);
        var delta = meter.Sample(3000, 700, 2);

        Assert.Equal((2000L, 200L), delta);
        Assert.Equal(2000, meter.Download);

        meter.Sample(10, 10, 3);
        Assert.Equal(0, meter.Download);
    }

    [Fact]
    public void SpeedMeter_history_keeps_last_samples_in_order()
    {
        var meter = new SpeedMeter();
        long total = 0;
        for (int i = 0; i < SpeedMeter.HistoryLength + 5; i++) meter.Sample(total += i, 0, i);

        Assert.Equal(SpeedMeter.HistoryLength, meter.Count);
        Assert.Equal(5, meter.History(0).Down);
        Assert.Equal(64, meter.History(SpeedMeter.HistoryLength - 1).Down);
    }

    [Fact]
    public void TrafficLedger_round_trips_and_sums_month()
    {
        string path = TempFile("traffic.json", "{\"2026-11-01\":[10,5],\"2026-10-31\":[100,100]}");
        var ledger = TrafficLedger.Load(path);
        ledger.Add(Noon, 7, 3);
        ledger.Save(path, Noon);

        var reloaded = TrafficLedger.Load(path);

        Assert.Equal(25, reloaded.MonthTotal(Noon));
        Assert.Equal((7d, 3d), reloaded.Day(Noon));
    }

    [Fact]
    public void Connectivity_needs_two_failures_to_go_offline()
    {
        var tracker = new ConnectivityTracker();

        Assert.Equal(ConnectivityChange.None, tracker.Report(null));
        Assert.Equal(ConnectivityChange.Lost, tracker.Report(null));
        Assert.Equal(ConnectivityChange.Restored, tracker.Report(40));
    }

    [Theory]
    [InlineData("9:05", true, "09:05")]
    [InlineData("23.59", true, "23:59")]
    [InlineData("", true, "")]
    [InlineData("24:00", false, "")]
    [InlineData("abc", false, "")]
    public void Note_time_is_validated(string input, bool valid, string expected)
    {
        Assert.Equal(valid, NoteStore.TryNormalizeTime(input, out var time));
        Assert.Equal(expected, time);
    }

    [Fact]
    public void Note_reminder_fires_once_and_persists()
    {
        string path = TempFile("notes.json");
        new NoteStore(path).Add(Noon, "11:59", "Uchrashuv");

        Assert.Equal("Uchrashuv", Assert.Single(new NoteStore(path).TakeDue(Noon)).Message);
        Assert.Empty(new NoteStore(path).TakeDue(Noon));
    }

    [Fact]
    public void Pomodoro_switches_mode_when_finished()
    {
        var timer = new PomodoroTimer(25, 5);
        timer.Toggle(Noon);

        Assert.Null(timer.Tick(Noon.AddMinutes(24)));
        Assert.NotNull(timer.Tick(Noon.AddMinutes(25)));
        Assert.Equal(PomodoroMode.Break, timer.Mode);
        Assert.Equal(300, timer.RemainingSeconds);
        Assert.False(timer.Running);
    }

    [Fact]
    public void Settings_reads_legacy_file_and_clamps_values()
    {
        string path = TempFile("config.json", "{\"unit\":\"mbyte\",\"fajr\":99,\"city\":\"Yo'q\",\"preMin\":15,\"limitGB\":50,\"lastTest\":\"1|2|3|x\"}");

        var settings = AppSettings.Load(path);

        Assert.Equal(SpeedUnit.Megabyte, settings.SpeedUnit());
        Assert.Equal(20, settings.Fajr);
        Assert.Equal("Toshkent", settings.City);
        Assert.Equal(15, settings.PreMin);
        Assert.Equal(50, settings.LimitGB);
        Assert.Equal(new[] { PrayerName.Peshin, PrayerName.Asr, PrayerName.Shom }, settings.NotifyPrayers());
    }

    [Fact]
    public void Settings_survives_corrupt_file() =>
        Assert.Equal("Toshkent", AppSettings.Load(TempFile("config.json", "{ buzilgan")).City);

    [Fact]
    public void Month_grid_starts_on_monday_with_iso_weeks()
    {
        Assert.Equal(new DateTime(2026, 10, 26), MonthGrid.FirstCell(2026, 11));
        Assert.Equal(53, MonthGrid.IsoWeek(new DateTime(2027, 1, 1)));
    }

    [Fact]
    public void Next_holiday_is_constitution_day() =>
        Assert.Equal(new DateTime(2026, 12, 8), new HolidayProvider().Next(Noon)!.Value.Date);

    [Theory]
    [InlineData(59, "59 daq")]
    [InlineData(61, "1 soat 1 daq")]
    public void Remaining_time_is_formatted(int minutes, string expected) =>
        Assert.Equal(expected, Format.Remaining(TimeSpan.FromMinutes(minutes)));
}
