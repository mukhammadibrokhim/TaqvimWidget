using System.Drawing;
using System.Drawing.Drawing2D;

namespace TaqvimWidget.App.UI;

internal static class DrawingExtensions
{
    public static GraphicsPath RoundedRectangle(float x, float y, float width, float height, float diameter)
    {
        var path = new GraphicsPath();
        path.AddArc(x, y, diameter, diameter, 180, 90);
        path.AddArc(x + width - diameter, y, diameter, diameter, 270, 90);
        path.AddArc(x + width - diameter, y + height - diameter, diameter, diameter, 0, 90);
        path.AddArc(x, y + height - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static void FillPill(this Graphics g, Brush brush, float x, float y, float width, float height)
    {
        using var path = RoundedRectangle(x, y, width, height, height - 1);
        g.FillPath(brush, path);
    }

    public static void Write(this Graphics g, string text, Font font, Brush brush, float x, float y) =>
        g.DrawString(text, font, brush, x, y);

    public static void Write(this Graphics g, string text, Font font, Brush brush, RectangleF area, StringFormat format) =>
        g.DrawString(text, font, brush, area, format);
}
