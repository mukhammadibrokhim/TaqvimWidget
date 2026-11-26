using System;
using System.Drawing;
using TaqvimWidget.Core.Formatting;
using TaqvimWidget.Core.Prayer;

namespace TaqvimWidget.App.UI.Cards;

internal sealed class PrayerCard : Card
{
    private const float CellWidth = 48.67f;

    public PrayerCard(WidgetState state, Theme theme)
        : base(state, theme)
    {
    }

    public override int Height => 76;
    public override bool Visible => State.Settings.Prayer;

    protected override void PaintContent(Graphics g, DateTime now)
    {
        var schedule = State.Prayers;
        var today = schedule.For(now);
        var next = schedule.Next(now);
        var current = schedule.Current(now);

        g.Write(schedule.City.Name, Theme.Tiny, Theme.Gray, 12, 9);
        string info = current is { } c
            ? $"Hozir: {c.Name} | {c.Next}gacha {Format.Remaining(c.End - now)}"
            : $"{next.Name} {Format.Clock(next.Time)} | {Format.Remaining(next.Time - now)}";
        g.Write(info, Theme.Tiny, Theme.AccentBrush, new RectangleF(68, 9, 236, 16), Theme.Right);

        for (var name = PrayerName.Bomdod; name <= PrayerName.Xufton; name++)
        {
            float x = 12 + (int)name * CellWidth;
            var label = Theme.Gray;
            var time = Theme.Text;

            if (current?.Name == name)
            {
                // Hozirgi vaqt: to'ldirilgan fon
                g.FillPill(Theme.AccentBrush, x + 1, 30, 46, 40);
                label = time = Theme.OnAccentBrush;
            }
            else if (next.Name == name && next.Time == today[name])
            {
                // Keyingi vaqt: faqat chegara
                using var outline = DrawingExtensions.RoundedRectangle(x + 1.5f, 30.5f, 45, 39, 19);
                g.DrawPath(Theme.AccentPen, outline);
                label = time = Theme.AccentBrush;
            }

            g.Write(name.ToString(), Theme.Tiny, label, new RectangleF(x, 33, CellWidth, 16), Theme.Center);
            g.Write(Format.Clock(today[name]), Theme.PrayerTime, time, new RectangleF(x, 49, CellWidth, 18), Theme.Center);
        }
    }
}
