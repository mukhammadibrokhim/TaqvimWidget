using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.IO;
using System.Windows.Forms;
using TaqvimWidget.App.Services;
using TaqvimWidget.App.UI;
using TaqvimWidget.Core;
using TaqvimWidget.Core.Formatting;
using TaqvimWidget.Core.Network;
using TaqvimWidget.Shared;

namespace TaqvimWidget.App;

/// <summary>Dastur hayot sikli: bitta 1 soniyalik taymer barcha vazifalarni boshqaradi.</summary>
internal sealed class WidgetContext : ApplicationContext
{
    private const int PingEveryTicks = 8;
    private const int ReminderEveryTicks = 15;
    private const int SaveTrafficEveryTicks = 60;

    private readonly bool _interactive;
    private readonly WidgetState _state;
    private readonly Theme _theme;
    private readonly NetworkSampler _network = new();
    private readonly PingMonitor _ping;
    private readonly TrayIcon _tray;
    private readonly Notifier _notifier;
    private readonly FlyoutForm _flyout;
    private readonly TaskbarOverlay _overlay;
    private readonly Timer _timer = new() { Interval = 1000 };
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly Stopwatch _sinceHidden = Stopwatch.StartNew();
    private DateTime _today = DateTime.Today;
    private int _tick;
    private bool _dialogOpen;
    private bool _disposed;

    public WidgetContext(bool interactive)
    {
        _interactive = interactive;
        _ping = new PingMonitor();
        _state = new WidgetState(_ping.State);
        _theme = Theme.FromSystem();

        var menu = new ContextMenuStrip();
        menu.Items.Add("Sozlamalar...", null, (_, _) => ShowSettings());
        menu.Items.Add("Bugunga qaytish", null, (_, _) => _flyout!.ShowToday());
        menu.Items.Add("Yopish", null, (_, _) => ExitThread());

        _tray = new TrayIcon(menu, interactive);
        _tray.Clicked += ToggleFlyout;
        _notifier = new Notifier(_tray.Icon);

        _flyout = new FlyoutForm(_state, _theme);
        _flyout.SettingsRequested += ShowSettings;
        _flyout.DayClicked += ShowNotes;
        _flyout.SpeedTestRequested += RunSpeedTest;
        _flyout.Deactivate += (_, _) =>
        {
            if (!_flyout.Visible || _dialogOpen) return;
            _flyout.Hide();
            _sinceHidden.Restart();
        };

        _overlay = new TaskbarOverlay(_state, _theme, menu);
        _overlay.Clicked += ToggleFlyout;

        if (!interactive) return;

        Notifier.Register(Application.ExecutablePath);
        _timer.Tick += (_, _) => OnTick();
        _timer.Start();
        OnTick();
        CheckReminders();
    }

    public void ShowFlyout()
    {
        _state.System.Restart();
        _state.System.Sample();
        _flyout.ShowAtTray();
    }

    public Bitmap RenderSnapshot()
    {
        _state.System.Sample();
        _state.Connectivity.Report(24);

        // Grafik bo'sh qolmasligi uchun namunaviy o'lchovlar
        long received = 0, sent = 0;
        for (int second = 0; second <= SpeedMeter.HistoryLength; second++)
        {
            received += (long)(2_600_000 + 1_900_000 * Math.Sin(second / 6.0));
            sent += (long)(420_000 + 260_000 * Math.Sin(second / 4.0));
            var delta = _state.Speed.Sample(received, sent, second);
            _state.Traffic.Add(DateTime.Now, delta.Received, delta.Sent);
        }

        System.Threading.Thread.Sleep(500);
        _state.System.Sample();
        return _flyout.RenderToBitmap();
    }

    private void ToggleFlyout()
    {
        if (_flyout.Visible) _flyout.Hide();
        // Tugma bosilganda oyna avval fokusni yo'qotib yopiladi: darhol qayta ochilmasin
        else if (_sinceHidden.ElapsedMilliseconds > 400) ShowFlyout();
    }

    private void OnTick()
    {
        try
        {
            _tick++;
            var now = DateTime.Now;

            var (received, sent, adaptersChanged) = _network.Read();
            if (adaptersChanged) _state.Speed.Reset();
            var delta = _state.Speed.Sample(received, sent, _clock.Elapsed.TotalSeconds);
            _state.Traffic.Add(now, delta.Received, delta.Sent);

            if (_tick % PingEveryTicks == 1) CheckConnectivity();
            if (_state.Pomodoro.Tick(now) is { } finished) _notifier.Show(finished);

            if (now.Date != _today)
            {
                _today = now.Date;
                _flyout.ShowToday();
            }

            if (_flyout.Visible)
            {
                _state.System.Sample();
                _flyout.Invalidate();
            }

            _overlay.Refresh(_state.Settings.Overlay);
            if (_tick % 2 == 0) UpdateTooltip();
            if (_tick % ReminderEveryTicks == 0) CheckReminders();
            if (_tick % SaveTrafficEveryTicks == 0) SaveTraffic();
        }
        catch (Exception ex)
        {
            Log.Write("tick: " + ex); // bitta xato fon dasturini to'xtatmasligi kerak
        }
    }

    private async void CheckConnectivity()
    {
        var change = await _ping.CheckAsync();
        if (_disposed || !_state.Settings.PingNotify) return;

        if (change == ConnectivityChange.Lost) _notifier.Show(new Notification("Internet uzildi", "Tarmoq javob bermayapti."));
        else if (change == ConnectivityChange.Restored) _notifier.Show(new Notification("Internet qayta tiklandi", $"Ping: {_ping.State.Latency} ms"));
    }

    private async void RunSpeedTest()
    {
        if (_state.SpeedTestRunning) return;
        _state.SpeedTestRunning = true;
        _flyout.Invalidate();

        Notification result;
        try
        {
            var (download, upload) = await SpeedTest.RunAsync();
            _state.Settings.LastSpeedTest = new Core.Settings.SpeedTestRecord
            {
                Download = download,
                Upload = upload,
                Time = DateTime.Now.ToString("s"),
            };
            SaveSettings();
            result = new Notification("Tezlik testi tayyor", $"Yuklash {download:N1} / Yuborish {upload:N1} Mbit/s");
        }
        catch (Exception ex) when (ex is WebException or IOException)
        {
            Log.Write("tezlik testi: " + ex.Message);
            result = new Notification("Tezlik testi bajarilmadi", "Internetni tekshirib, qayta urinib ko'ring.");
        }

        if (_disposed) return;
        _state.SpeedTestRunning = false;
        _notifier.Show(result);
        _flyout.Invalidate();
    }

    private void CheckReminders()
    {
        var now = DateTime.Now;
        foreach (var note in _state.Notes.TakeDue(now)) _notifier.Show(note);

        var settings = _state.Settings;
        if (!settings.PrayerNotify) return;
        foreach (var prayer in _state.PrayerPlanner.Due(now, _state.Prayers, settings.NotifyPrayers(), settings.PreMin, settings.AtTime))
        {
            _notifier.Show(prayer);
        }
    }

    private void UpdateTooltip()
    {
        var unit = _state.Settings.SpeedUnit();
        var down = Format.Speed(_state.Speed.Download, unit);
        var up = Format.Speed(_state.Speed.Upload, unit);
        string text = $"↓ {down.Value} {down.Unit}  ↑ {up.Value} {up.Unit}";
        if (_state.Pomodoro.Running) text += "  |  " + Format.MinutesSeconds(_state.Pomodoro.RemainingSeconds);
        _tray.SetText(text);
    }

    private void ShowNotes(DateTime date) => RunDialog(() =>
    {
        using var dialog = new NotesDialog(_theme, _state.Notes, date);
        dialog.ShowDialog();
    });

    private void ShowSettings() => RunDialog(() =>
    {
        using var dialog = new SettingsDialog(_theme, _state.Settings, Autostart.Enabled);
        dialog.TestNotificationRequested += () => _notifier.Show(new Notification("Sinov bildirishnomasi", "Taqvim Widget bildirishnomalari ishlayapti."));
        if (dialog.ShowDialog() != DialogResult.OK) return;

        dialog.ApplyTo(_state.Settings);
        Autostart.Set(dialog.AutostartSelected);
        SaveSettings();
        _state.ApplySettings();
        _flyout.UpdateLayout();
        _overlay.Refresh(_state.Settings.Overlay);
    });

    private void RunDialog(Action show)
    {
        if (_dialogOpen) return;
        _dialogOpen = true;
        try
        {
            show();
        }
        finally
        {
            _dialogOpen = false;
        }

        _flyout.Invalidate();
        if (_flyout.Visible) _flyout.Activate();
    }

    private void SaveSettings() => _state.Settings.Save(AppPaths.Settings, Log.Write);

    private void SaveTraffic() => _state.Traffic.Save(AppPaths.Traffic, DateTime.Today, Log.Write);

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _disposed = true;
            _timer.Dispose();
            if (_interactive) SaveTraffic();
            _overlay.Dispose();
            _flyout.Dispose();
            _tray.Dispose();
            _ping.Dispose();
            _network.Dispose();
            _theme.Dispose();
        }

        base.Dispose(disposing);
    }
}
