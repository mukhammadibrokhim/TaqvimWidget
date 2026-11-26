using TaqvimWidget.App.Services;
using TaqvimWidget.Core.Calendar;
using TaqvimWidget.Core.Network;
using TaqvimWidget.Core.Notes;
using TaqvimWidget.Core.Pomodoro;
using TaqvimWidget.Core.Prayer;
using TaqvimWidget.Core.Settings;

namespace TaqvimWidget.App;

/// <summary>Interfeys chizadigan barcha ma'lumotlar.</summary>
internal sealed class WidgetState
{
    public WidgetState(ConnectivityTracker connectivity)
    {
        Connectivity = connectivity;
        Settings = AppSettings.Load(AppPaths.Settings, Log.Write);
        Traffic = TrafficLedger.Load(AppPaths.Traffic, Log.Write);
        Notes = new NoteStore(AppPaths.Notes, Log.Write);
        Prayers = new PrayerSchedule(Cities.Find(Settings.City), Settings.PrayerParameters());
        Pomodoro = new PomodoroTimer(Settings.FocusMin, Settings.BreakMin);
    }

    public AppSettings Settings { get; }
    public SpeedMeter Speed { get; } = new();
    public TrafficLedger Traffic { get; }
    public ConnectivityTracker Connectivity { get; }
    public SystemMonitor System { get; } = new();
    public NoteStore Notes { get; }
    public HolidayProvider Holidays { get; } = new();
    public PrayerSchedule Prayers { get; }
    public PrayerNotificationPlanner PrayerPlanner { get; } = new();
    public PomodoroTimer Pomodoro { get; }
    public bool SpeedTestRunning { get; set; }

    public void ApplySettings()
    {
        Prayers.Configure(Cities.Find(Settings.City), Settings.PrayerParameters());
        PrayerPlanner.Reset();
        Pomodoro.Configure(Settings.FocusMin, Settings.BreakMin);
    }
}
