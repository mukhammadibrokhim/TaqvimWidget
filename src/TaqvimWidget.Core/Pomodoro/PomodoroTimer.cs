using System;

namespace TaqvimWidget.Core.Pomodoro;

public enum PomodoroMode
{
    Focus,
    Break,
}

public sealed class PomodoroTimer
{
    private int _focusMinutes;
    private int _breakMinutes;
    private DateTime _endsAt;

    public PomodoroTimer(int focusMinutes, int breakMinutes)
    {
        _focusMinutes = focusMinutes;
        _breakMinutes = breakMinutes;
        RemainingSeconds = Duration;
    }

    public PomodoroMode Mode { get; private set; } = PomodoroMode.Focus;
    public bool Running { get; private set; }
    public int RemainingSeconds { get; private set; }

    private int Duration => (Mode == PomodoroMode.Focus ? _focusMinutes : _breakMinutes) * 60;

    public void Configure(int focusMinutes, int breakMinutes)
    {
        _focusMinutes = focusMinutes;
        _breakMinutes = breakMinutes;
        if (!Running) RemainingSeconds = Duration;
    }

    public void Toggle(DateTime now)
    {
        if (Running)
        {
            RemainingSeconds = Math.Max(0, (int)Math.Ceiling((_endsAt - now).TotalSeconds));
            Running = false;
        }
        else
        {
            _endsAt = now.AddSeconds(RemainingSeconds);
            Running = true;
        }
    }

    public void Reset()
    {
        Running = false;
        RemainingSeconds = Duration;
    }

    public void SwitchMode()
    {
        if (Running) return;
        Mode = Mode == PomodoroMode.Focus ? PomodoroMode.Break : PomodoroMode.Focus;
        RemainingSeconds = Duration;
    }

    /// <returns>Bosqich tugagan bo'lsa bildirishnoma.</returns>
    public Notification? Tick(DateTime now)
    {
        if (!Running) return null;

        double left = (_endsAt - now).TotalSeconds;
        if (left > 0)
        {
            RemainingSeconds = (int)Math.Ceiling(left);
            return null;
        }

        Running = false;
        var message = Mode == PomodoroMode.Focus
            ? new Notification("Fokus vaqti tugadi", "Biroz dam oling.")
            : new Notification("Dam olish tugadi", "Ishga qaytish vaqti.");
        SwitchMode();
        return message;
    }
}
