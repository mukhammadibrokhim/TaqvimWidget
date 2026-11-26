using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using TaqvimWidget.App.Interop;
using TaqvimWidget.App.UI.Cards;
using TaqvimWidget.Core.Formatting;

namespace TaqvimWidget.App.UI;

/// <summary>
/// Vazifalar panelida soat yonidagi tezlik yozuvi. Matn yoki holat o'zgargandagina qayta chiziladi.
/// </summary>
internal sealed class TaskbarOverlay : Form
{
    private const int LogicalWidth = 88;
    private const int LogicalHeight = 38;

    private readonly WidgetState _state;
    private readonly Theme _theme;
    private readonly float _scale;
    private (string Value, string Unit) _down = ("0", "Kb/s");
    private (string Value, string Unit) _up = ("0", "Kb/s");
    private Brush _status;
    private Rectangle _bounds;
    private bool _hover;

    public TaskbarOverlay(WidgetState state, Theme theme, ContextMenuStrip menu)
    {
        _state = state;
        _theme = theme;
        _status = theme.Dim;
        using (var screen = Graphics.FromHwnd(IntPtr.Zero)) _scale = screen.DpiX / 96f;

        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        DoubleBuffered = true;
        // Fon panel rangiga yaqin va shaffof: alohida quti ko'rinmaydi
        BackColor = theme.Background;
        TransparencyKey = theme.Background;
        ContextMenuStrip = menu;
        Size = new Size(Scale_(LogicalWidth), Scale_(LogicalHeight));
    }

    public event Action? Clicked;

    protected override bool ShowWithoutActivation => true;

    // Fokusni olmaydi va Alt+Tab ro'yxatida ko'rinmaydi
    protected override CreateParams CreateParams
    {
        get
        {
            var parameters = base.CreateParams;
            parameters.ExStyle |= NativeMethods.WsExNoActivate | NativeMethods.WsExToolWindow;
            return parameters;
        }
    }

    public void Refresh(bool enabled)
    {
        if (!enabled || IsFullScreenActive() || !TryGetPosition(out var position))
        {
            if (Visible) Hide();
            return;
        }

        bool wasVisible = Visible;
        if (!wasVisible) Show();

        var bounds = new Rectangle(position, Size);
        // Vazifalar paneli oynani to'sib qo'yishi mumkin, shu sabab har safar eng yuqoriga qo'yiladi (arzon amal)
        NativeMethods.SetWindowPos(Handle, NativeMethods.HwndTopMost, bounds.X, bounds.Y, bounds.Width, bounds.Height,
            NativeMethods.SwpNoActivate | NativeMethods.SwpShowWindow);

        var unit = _state.Settings.SpeedUnit();
        var down = Shorten(Format.Speed(_state.Speed.Download, unit));
        var up = Shorten(Format.Speed(_state.Speed.Upload, unit));
        var status = PingBrush.For(_state.Connectivity, _theme);

        if (wasVisible && down == _down && up == _up && status == _status && bounds == _bounds) return;
        (_down, _up, _status, _bounds) = (down, up, status, bounds);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        g.ScaleTransform(_scale, _scale);

        if (_hover)
        {
            using var path = DrawingExtensions.RoundedRectangle(0, 0, LogicalWidth - 2, LogicalHeight - 2, 12);
            g.FillPath(_theme.OverlayHover, path);
        }

        PaintRow(g, "↓", _theme.Down, _down, 3);
        PaintRow(g, "↑", _theme.Up, _up, 19);
        g.FillEllipse(_status, 79, 15, 6, 6);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hover = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hover = false;
        Invalidate();
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button == MouseButtons.Left) Clicked?.Invoke();
    }

    private static (string, string) Shorten((string Value, string Unit) speed) => (speed.Value, speed.Unit.Replace("bit/s", "b/s"));

    private void PaintRow(Graphics g, string arrow, Brush brush, (string Value, string Unit) speed, int y)
    {
        g.Write(arrow, _theme.OverlayArrow, brush, 11, y - 1);
        g.Write(speed.Value, _theme.OverlayValue, _theme.Text, new RectangleF(14, y, 34, 18), _theme.Right);
        g.Write(speed.Unit, _theme.OverlayUnitFont, _theme.OverlayUnit, 49, y + 2);
    }

    // 2 - to'liq ekranli ilova, 3 - D3D o'yin, 4 - taqdimot rejimi
    private static bool IsFullScreenActive() =>
        NativeMethods.SHQueryUserNotificationState(out int state) == 0 && state is >= 2 and <= 4;

    private bool TryGetPosition(out Point position)
    {
        position = default;
        var taskbar = NativeMethods.FindWindow("Shell_TrayWnd", null);
        if (taskbar == IntPtr.Zero || !NativeMethods.GetWindowRect(taskbar, out var bar)) return false;

        var tray = NativeMethods.FindWindowEx(taskbar, IntPtr.Zero, "TrayNotifyWnd", null);
        int x = tray != IntPtr.Zero && NativeMethods.GetWindowRect(tray, out var notify)
            ? notify.Left - Width - Scale_(4)
            : bar.Right - Scale_(330);
        position = new Point(x, bar.Top + (bar.Bottom - bar.Top - Height) / 2);
        return true;
    }

    private int Scale_(int value) => (int)Math.Round(value * _scale);
}
