using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using TaqvimWidget.App.Interop;
using TaqvimWidget.App.Services;
using TaqvimWidget.App.UI.Cards;

namespace TaqvimWidget.App.UI;

/// <summary>Soat yonida ochiladigan asosiy oyna.</summary>
internal sealed class FlyoutForm : Form
{
    private const int LogicalWidth = 340;
    private const int Margin_ = 12;
    private const int Gap = 10;

    private readonly Theme _theme;
    private readonly CalendarCard _calendar;
    private readonly IReadOnlyList<Card> _cards;
    private readonly float _dpiScale;
    private float _scale;

    public FlyoutForm(WidgetState state, Theme theme)
    {
        _theme = theme;
        using (var screen = Graphics.FromHwnd(IntPtr.Zero)) _dpiScale = screen.DpiX / 96f;
        _scale = _dpiScale;

        _calendar = new CalendarCard(state, theme, date => DayClicked?.Invoke(date));
        _cards = new Card[]
        {
            new SpeedCard(state, theme, () => SettingsRequested?.Invoke()),
            new PrayerCard(state, theme),
            new SystemCard(state, theme, () => SpeedTestRequested?.Invoke()),
            _calendar,
            new PomodoroCard(state, theme),
        };

        Text = Shared.AppIdentity.Name;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        BackColor = theme.Background;
        DoubleBuffered = true;
        UpdateLayout();
    }

    public event Action? SettingsRequested;
    public event Action? SpeedTestRequested;
    public event Action<DateTime>? DayClicked;

    private int LogicalHeight => _cards.Where(c => c.Visible).Sum(c => c.Height + Gap) + Margin_ * 2 - Gap + 2;

    public void ShowAtTray()
    {
        _calendar.ShowToday();
        UpdateLayout();
        Show();
        Activate();
    }

    public void ShowToday()
    {
        _calendar.ShowToday();
        Invalidate();
    }

    /// <summary>Ko'rinadigan kartalarga qarab o'lchamni yangilaydi; ekranga sig'masa kichraytiradi.</summary>
    public void UpdateLayout()
    {
        var area = Screen.FromPoint(Cursor.Position).WorkingArea;
        float needed = LogicalHeight * _dpiScale;
        float available = area.Height - 24;
        _scale = needed > available ? _dpiScale * available / needed : _dpiScale;

        Size = new Size((int)Math.Ceiling(LogicalWidth * _scale), (int)Math.Ceiling(LogicalHeight * _scale));
        Location = new Point(area.Right - Width - Margin_, area.Bottom - Height - Margin_);
        Invalidate();
    }

    public Bitmap RenderToBitmap()
    {
        UpdateLayout();
        var image = new Bitmap(Width, Height);
        using var g = Graphics.FromImage(image);
        g.Clear(_theme.Background);
        Render(g);
        return image;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyWindows11Style();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        try
        {
            Render(e.Graphics);
        }
        catch (Exception ex) when (ex is ArgumentException or System.Runtime.InteropServices.ExternalException)
        {
            Log.Write("chizish: " + ex.Message);
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;

        var point = new PointF(e.X / _scale, e.Y / _scale);
        float top = Margin_;
        foreach (var card in _cards.Where(c => c.Visible))
        {
            if (card.Click(new PointF(point.X - Margin_, point.Y - top)))
            {
                Invalidate();
                return;
            }

            top += card.Height + Gap;
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        _calendar.Move(e.Delta > 0 ? -1 : 1);
        Invalidate();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Alt+F4 dasturni yopmaydi, faqat oynani yashiradi
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
        }

        base.OnFormClosing(e);
    }

    private void Render(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        var now = DateTime.Now;
        float top = Margin_;
        foreach (var card in _cards.Where(c => c.Visible))
        {
            g.ResetTransform();
            g.ScaleTransform(_scale, _scale);
            g.TranslateTransform(Margin_, top);
            card.Paint(g, now);
            top += card.Height + Gap;
        }
    }

    // Yumaloq burchak va Mica fon (qo'llab-quvvatlanmasa oddiy fon qoladi)
    private void ApplyWindows11Style()
    {
        DwmStyle.Set(Handle, NativeMethods.DwmWindowCornerPreference, 2);
        DwmStyle.Set(Handle, NativeMethods.DwmUseImmersiveDarkMode, _theme.Light ? 0 : 1);
        DwmStyle.Set(Handle, NativeMethods.DwmBorderColor, _theme.Light ? 0xDCDCDC : 0x504A4A);

        var margins = new NativeMethods.Margins { Left = -1, Right = -1, Top = -1, Bottom = -1 };
        bool extended = NativeMethods.DwmExtendFrameIntoClientArea(Handle, ref margins) == 0;
        if (extended && DwmStyle.Set(Handle, NativeMethods.DwmSystemBackdropType, 2)) BackColor = Color.Black;
    }
}

internal static class DwmStyle
{
    public static bool Set(IntPtr window, int attribute, int value) =>
        NativeMethods.DwmSetWindowAttribute(window, attribute, ref value, sizeof(int)) == 0;
}
