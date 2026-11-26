using System;
using System.Drawing;
using System.Windows.Forms;

namespace TaqvimWidget.App.UI;

internal sealed class TrayIcon : IDisposable
{
    private const int MaxTooltipLength = 63;
    private readonly System.Drawing.Icon _image;

    public TrayIcon(ContextMenuStrip menu, bool visible)
    {
        _image = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? (System.Drawing.Icon)SystemIcons.Application.Clone();
        Icon = new NotifyIcon
        {
            Icon = _image,
            Text = Shared.AppIdentity.Name,
            ContextMenuStrip = menu,
            Visible = visible,
        };
        Icon.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left) Clicked?.Invoke();
        };
    }

    public event Action? Clicked;

    public NotifyIcon Icon { get; }

    public void SetText(string text)
    {
        if (text.Length > MaxTooltipLength) text = text.Substring(0, MaxTooltipLength);
        if (Icon.Text != text) Icon.Text = text;
    }

    public void Dispose()
    {
        Icon.Visible = false;
        Icon.Dispose();
        _image.Dispose();
    }
}
