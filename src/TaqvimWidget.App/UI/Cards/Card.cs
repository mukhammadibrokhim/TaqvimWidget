using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TaqvimWidget.App.UI.Cards;

/// <summary>
/// Widgetdagi bitta karta. Koordinatalar kartaning chap yuqori burchagiga nisbatan,
/// 96 DPI mantiqiy piksellarda.
/// </summary>
internal abstract class Card
{
    public const int Width = 316;

    private readonly List<(RectangleF Area, Action Action)> _hotspots = new();

    protected Card(WidgetState state, Theme theme)
    {
        State = state;
        Theme = theme;
    }

    protected WidgetState State { get; }
    protected Theme Theme { get; }

    public abstract int Height { get; }
    public abstract bool Visible { get; }

    protected abstract void PaintContent(Graphics g, DateTime now);

    public void Paint(Graphics g, DateTime now)
    {
        var bounds = new RectangleF(0, 0, Width, Height);
        using (var path = DrawingExtensions.RoundedRectangle(0, 0, Width, Height, 16))
        using (var fill = new LinearGradientBrush(bounds, Theme.Card, Theme.CardBottom, 90f))
        {
            g.FillPath(fill, path);
            g.DrawPath(Theme.BorderPen, path);
        }

        PaintContent(g, now);
    }

    /// <returns>Bosilgan joyda tugma bo'lsa true.</returns>
    public bool Click(PointF point)
    {
        foreach (var (area, action) in _hotspots)
        {
            if (!area.Contains(point)) continue;
            action();
            return true;
        }

        return false;
    }

    protected void AddHotspot(RectangleF area, Action action) => _hotspots.Add((area, action));
}
