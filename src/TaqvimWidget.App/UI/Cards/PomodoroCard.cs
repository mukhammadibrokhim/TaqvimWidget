using System;
using System.Drawing;
using TaqvimWidget.Core.Formatting;
using TaqvimWidget.Core.Pomodoro;

namespace TaqvimWidget.App.UI.Cards;

internal sealed class PomodoroCard : Card
{
    private static readonly RectangleF ModeButton = new(146, 14, 72, 32);
    private static readonly RectangleF PlayButton = new(228, 14, 36, 32);
    private static readonly RectangleF ResetButton = new(270, 14, 34, 32);

    public PomodoroCard(WidgetState state, Theme theme)
        : base(state, theme)
    {
        AddHotspot(ModeButton, () => State.Pomodoro.SwitchMode());
        AddHotspot(PlayButton, () => State.Pomodoro.Toggle(DateTime.Now));
        AddHotspot(ResetButton, () => State.Pomodoro.Reset());
    }

    public override int Height => 60;
    public override bool Visible => State.Settings.Timer;

    protected override void PaintContent(Graphics g, DateTime now)
    {
        var timer = State.Pomodoro;
        bool focus = timer.Mode == PomodoroMode.Focus;

        g.Write(focus ? "Fokus" : "Dam olish", Theme.Tiny, Theme.Gray, 12, 7);
        g.Write(Format.MinutesSeconds(timer.RemainingSeconds), Theme.Timer, timer.Running ? Theme.AccentBrush : Theme.Text, 10, 20);

        Button(g, ModeButton, Theme.Track);
        g.Write(focus ? "Dam" : "Fokus", Theme.Tiny, Theme.Text, ModeButton, Theme.Center);
        Button(g, PlayButton, Theme.AccentBrush);
        g.Write(timer.Running ? "" : "", Theme.Icon, Theme.OnAccentBrush, PlayButton, Theme.Center);
        Button(g, ResetButton, Theme.Track);
        g.Write("", Theme.Icon, Theme.Text, ResetButton, Theme.Center);
    }

    private static void Button(Graphics g, RectangleF area, Brush brush) =>
        g.FillPill(brush, area.X, area.Y, area.Width, area.Height);
}
