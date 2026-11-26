using System;
using System.Drawing;
using System.Globalization;

namespace TaqvimWidget.App.UI.Cards;

internal sealed class SystemCard : Card
{
    private static readonly RectangleF TestButton = new(180, 58, 124, 28);

    public SystemCard(WidgetState state, Theme theme, Action startSpeedTest)
        : base(state, theme) =>
        AddHotspot(TestButton, startSpeedTest);

    public override int Height => 96;
    public override bool Visible => State.Settings.System;

    protected override void PaintContent(Graphics g, DateTime now)
    {
        var system = State.System;
        PaintMeter(g, "CPU", $"{system.Cpu}%", system.Cpu, Level(system.Cpu, 60, 85), 12);
        PaintMeter(g, "RAM", $"{system.Memory}%", system.Memory, Level(system.Memory, 75, 90), 114);

        if (system.Battery is { } battery)
        {
            var brush = system.PluggedIn ? Theme.Green : battery <= 20 ? Theme.Red : Theme.AccentBrush;
            PaintMeter(g, "Zaryad", $"{battery}%{(system.PluggedIn ? " +" : "")}", battery, brush, 216);
        }
        else
        {
            g.Write("Zaryad", Theme.Tiny, Theme.Gray, 216, 10);
            g.Write("yo'q", Theme.Tiny, Theme.Dim, new RectangleF(216, 10, 88, 16), Theme.Right);
        }

        PaintSpeedTest(g, now);
    }

    private Brush Level(int percent, int warning, int critical) =>
        percent >= critical ? Theme.Red : percent >= warning ? Theme.Amber : Theme.AccentBrush;

    private void PaintMeter(Graphics g, string label, string value, int percent, Brush brush, float x)
    {
        g.Write(label, Theme.Tiny, Theme.Gray, x, 10);
        g.Write(value, Theme.Tiny, Theme.Text, new RectangleF(x, 10, 88, 16), Theme.Right);
        g.FillPill(Theme.Track, x, 30, 88, 6);
        g.FillPill(brush, x, 30, Math.Max(6f, Math.Min(88f, 88f * percent / 100f)), 6);
    }

    private void PaintSpeedTest(Graphics g, DateTime now)
    {
        string result = "Hali o'lchanmagan";
        string details = string.Empty;

        if (State.SpeedTestRunning)
        {
            result = "O'lchanmoqda...";
            details = "bir daqiqagacha";
        }
        else if (State.Settings.LastSpeedTest is { } last
                 && DateTime.TryParse(last.Time, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
        {
            result = $"↓ {last.Download:N1}  ↑ {last.Upload:N1} Mbit/s";
            details = "Oxirgi test: " + (time.Date == now.Date ? "bugun " + time.ToString("HH:mm") : time.ToString("dd.MM HH:mm"));
        }

        g.Write(result, Theme.Small, Theme.Text, 12, 58);
        g.Write(details, Theme.Tiny, Theme.Gray, 12, 76);
        g.FillPill(State.SpeedTestRunning ? Theme.Track : Theme.AccentBrush, TestButton.X, TestButton.Y, TestButton.Width, TestButton.Height);
        g.Write("Tezlikni o'lchash", Theme.Tiny, State.SpeedTestRunning ? Theme.Gray : Theme.OnAccentBrush, TestButton, Theme.Center);
    }
}
