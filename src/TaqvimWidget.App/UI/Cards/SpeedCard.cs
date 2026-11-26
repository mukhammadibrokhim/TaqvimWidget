using System;
using System.Drawing;
using TaqvimWidget.Core.Formatting;
using TaqvimWidget.Core.Network;

namespace TaqvimWidget.App.UI.Cards;

internal sealed class SpeedCard : Card
{
    private const float ChartTop = 106;
    private const float ChartHeight = 22;
    private const float ChartWidth = 292;
    private const double MinChartScale = 125_000; // 1 Mbit/s: sokin paytda shovqin kattalashib ketmasin

    private readonly PointF[] _downPoints = new PointF[SpeedMeter.HistoryLength];
    private readonly PointF[] _upPoints = new PointF[SpeedMeter.HistoryLength];

    public SpeedCard(WidgetState state, Theme theme, Action openSettings)
        : base(state, theme) =>
        AddHotspot(new RectangleF(272, 2, 36, 28), openSettings);

    public override int Height => 156;
    public override bool Visible => true;

    protected override void PaintContent(Graphics g, DateTime now)
    {
        g.Write("Internet tezligi", Theme.Tiny, Theme.Gray, 12, 9);
        PaintPing(g);
        g.Write("", Theme.Icon, Theme.Gray, new RectangleF(276, 4, 28, 24), Theme.Center);

        g.FillPolygon(Theme.Down, new[] { new Point(12, 34), new Point(20, 34), new Point(16, 40) });
        g.FillPolygon(Theme.Up, new[] { new Point(170, 40), new Point(178, 40), new Point(174, 34) });
        PaintSpeed(g, "Yuklash", Theme.Down, State.Speed.Download, 12);
        PaintSpeed(g, "Yuborish", Theme.Up, State.Speed.Upload, 170);

        PaintChart(g);
        PaintTraffic(g, now);
    }

    private void PaintPing(Graphics g)
    {
        var connection = State.Connectivity;
        string text = !connection.Online ? "Offlayn" : connection.Latency is { } ms ? $"{ms} ms" : "Ping ...";
        float width = g.MeasureString(text, Theme.Tiny).Width;
        g.Write(text, Theme.Tiny, Theme.Text, 272 - width, 9);
        g.FillEllipse(PingBrush.For(connection, Theme), 261 - width, 14, 7, 7);
    }

    private void PaintSpeed(Graphics g, string label, Brush brush, double bytesPerSecond, float x)
    {
        var unit = State.Settings.SpeedUnit();
        var (value, unitName) = Format.Speed(bytesPerSecond, unit);
        g.Write(label, Theme.Tiny, Theme.Gray, x + 13, 30);
        g.Write(value, Theme.Big, Theme.Text, x - 3, 44);
        g.Write(unitName, Theme.Tiny, brush, x + g.MeasureString(value, Theme.Big).Width - 8, 63);
        g.Write(Format.AlternativeSpeed(bytesPerSecond, unit), Theme.Tiny, Theme.Gray, x, 86);
    }

    private void PaintChart(Graphics g)
    {
        var meter = State.Speed;
        if (meter.Count < 2) return;

        double max = MinChartScale;
        for (int i = 0; i < meter.Count; i++)
        {
            var (down, up) = meter.History(i);
            max = Math.Max(max, Math.Max(down, up));
        }

        const float step = ChartWidth / (SpeedMeter.HistoryLength - 1);
        int offset = SpeedMeter.HistoryLength - meter.Count;
        for (int i = 0; i < SpeedMeter.HistoryLength; i++)
        {
            // Ishlatilmagan nuqtalar birinchi nuqta ustiga qo'yiladi (massiv qayta ajratilmaydi)
            int sample = Math.Max(0, i - offset);
            var (down, up) = meter.History(sample);
            float x = 12 + (offset + sample) * step;
            _downPoints[i] = new PointF(x, ChartTop + ChartHeight - (float)(down / max) * ChartHeight);
            _upPoints[i] = new PointF(x, ChartTop + ChartHeight - (float)(up / max) * ChartHeight);
        }

        g.DrawLines(Theme.DownPen, _downPoints);
        g.DrawLines(Theme.UpPen, _upPoints);
    }

    private void PaintTraffic(Graphics g, DateTime now)
    {
        var (received, sent) = State.Traffic.Day(now);
        g.Write($"Bugun  ↓ {Format.Bytes(received)}  ↑ {Format.Bytes(sent)}", Theme.Tiny, Theme.Gray, 12, 134);

        double month = State.Traffic.MonthTotal(now);
        double limit = State.Settings.LimitGB;
        string text = "Oy: " + Format.Bytes(month);
        var brush = Theme.Gray;
        if (limit > 0)
        {
            text += $" / {limit:N0} GB";
            if (month >= 0.9 * limit * 1024 * 1024 * 1024) brush = Theme.Red;
        }

        g.Write(text, Theme.Tiny, brush, new RectangleF(158, 134, 146, 16), Theme.Right);
    }
}

internal static class PingBrush
{
    public static Brush For(ConnectivityTracker connection, Theme theme)
    {
        if (!connection.Online) return theme.Red;
        return connection.Latency switch
        {
            null => theme.Dim,
            < 100 => theme.Green,
            < 250 => theme.Amber,
            _ => theme.Red,
        };
    }
}
