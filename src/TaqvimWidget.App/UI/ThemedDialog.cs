using System;
using System.Drawing;
using System.Windows.Forms;
using TaqvimWidget.App.Interop;

namespace TaqvimWidget.App.UI;

internal abstract class ThemedDialog : Form
{
    private readonly float _scale;

    protected ThemedDialog(Theme theme, string title, int width, int height)
    {
        Theme = theme;
        using (var screen = Graphics.FromHwnd(IntPtr.Zero)) _scale = screen.DpiX / 96f;

        Text = title;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        TopMost = true;
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(Px(width), Px(height));
        BackColor = theme.Background;
        ForeColor = theme.TextColor;
        // Dialog shrifti punktda bo'lishi kerak, aks holda ichki elementlar noto'g'ri o'lchanadi
        Font = new Font(theme.TextFamily, 9.5f);
    }

    protected Theme Theme { get; }

    protected int Px(int value) => (int)Math.Round(value * _scale);

    protected T Add<T>(Control parent, int x, int y, int width, int height, string? text = null)
        where T : Control, new()
    {
        var control = new T { Location = new Point(Px(x), Px(y)), Size = new Size(Px(width), Px(height)) };
        if (text is not null) control.Text = text;
        if (control is TextBox or ListBox or ComboBox or NumericUpDown)
        {
            control.BackColor = Theme.Input;
            control.ForeColor = Theme.TextColor;
        }

        parent.Controls.Add(control);
        return control;
    }

    protected Button AddButton(int x, int y, int width, string text, bool primary = false)
    {
        var button = Add<Button>(this, x, y, width, 32, text);
        button.FlatStyle = FlatStyle.Flat;
        button.UseVisualStyleBackColor = false;
        button.FlatAppearance.BorderColor = Theme.CardBorder;
        button.BackColor = primary ? Theme.Accent : Theme.Card;
        button.ForeColor = primary ? Theme.OnAccent : Theme.TextColor;
        return button;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        DwmStyle.Set(Handle, NativeMethods.DwmUseImmersiveDarkMode, Theme.Light ? 0 : 1);
    }
}
