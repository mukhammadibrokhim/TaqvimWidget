using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using Microsoft.Win32;

namespace TaqvimWidget.App.UI;

/// <summary>
/// Windows mavzusi va aksent rangiga mos ranglar. Barcha GDI obyektlari bir marta yaratiladi:
/// har chizishda yangi obyekt ajratilmaydi.
/// </summary>
internal sealed class Theme : IDisposable
{
    private readonly List<IDisposable> _owned = new();

    private Theme(bool light, Color accent)
    {
        Light = light;
        Accent = accent;
        OnAccent = Luminance(accent) > 130 ? Color.Black : Color.White;

        Background = Pick(Rgb(243, 243, 243), Rgb(32, 32, 32));
        TextColor = Pick(Rgb(20, 20, 20), Color.White);
        GrayColor = Pick(Rgb(80, 80, 86), Rgb(196, 196, 202));
        Card = Pick(Color.White, Rgb(62, 62, 68));
        CardBottom = Pick(Rgb(248, 248, 250), Rgb(48, 48, 54));
        CardBorder = Pick(Rgb(215, 215, 222), Rgb(88, 88, 96));
        Input = Pick(Color.White, Rgb(66, 66, 72));

        Text = Brush(TextColor);
        Gray = Brush(GrayColor);
        Dim = Brush(Pick(Rgb(165, 165, 170), Rgb(120, 120, 126)));
        AccentBrush = Brush(accent);
        OnAccentBrush = Brush(OnAccent);
        Red = Brush(Rgb(255, 110, 110));
        Amber = Brush(Rgb(255, 200, 80));
        Green = Brush(Rgb(90, 220, 150));
        Down = Brush(Rgb(90, 225, 160));
        Up = Brush(Rgb(120, 185, 255));
        Track = Brush(Pick(Rgb(225, 225, 230), Rgb(40, 40, 44)));
        OverlayUnit = Brush(Pick(Rgb(100, 100, 100), Rgb(170, 170, 170)));
        OverlayHover = Brush(Pick(Rgb(228, 228, 228), Rgb(52, 52, 52)));

        BorderPen = Own(new Pen(CardBorder, 1));
        AccentPen = Own(new Pen(accent, 1.5f));
        DownPen = Own(new Pen(Rgb(90, 225, 160), 1.8f));
        UpPen = Own(new Pen(Rgb(120, 185, 255), 1.8f));

        using var installed = new InstalledFontCollection();
        var names = new HashSet<string>(installed.Families.Select(f => f.Name));
        string FirstOf(params string[] candidates) => candidates.FirstOrDefault(names.Contains) ?? "Segoe UI";

        TextFamily = FirstOf("Segoe UI Variable Text", "Segoe UI");
        string display = FirstOf("Segoe UI Variable Display", "Segoe UI Semibold");
        string icons = FirstOf("Segoe Fluent Icons", "Segoe MDL2 Assets");

        Tiny = Font(TextFamily, 9);
        Small = Font(TextFamily, 10.5f);
        Big = Font(display, 24, FontStyle.Bold);
        Heading = Font(display, 13, FontStyle.Bold);
        PrayerTime = Font(display, 10.5f, FontStyle.Bold);
        Timer = Font(display, 22, FontStyle.Bold);
        Arrow = Font(TextFamily, 15);
        Icon = Font(icons, 13);
        OverlayValue = Font(TextFamily, 9);
        OverlayUnitFont = Font(TextFamily, 7.5f);
        OverlayArrow = Font(TextFamily, 9, FontStyle.Bold);
        Dialog = Font(TextFamily, 9.5f);

        Center = Own(new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.NoClip) { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.None });
        Left = Own(new StringFormat { LineAlignment = StringAlignment.Center });
        Right = Own(new StringFormat { Alignment = StringAlignment.Far });
    }

    public bool Light { get; }
    public Color Accent { get; }
    public Color OnAccent { get; }
    public Color Background { get; }
    public Color TextColor { get; }
    public Color GrayColor { get; }
    public Color Card { get; }
    public Color CardBottom { get; }
    public Color CardBorder { get; }
    public Color Input { get; }
    public string TextFamily { get; }

    public Brush Text { get; }
    public Brush Gray { get; }
    public Brush Dim { get; }
    public Brush AccentBrush { get; }
    public Brush OnAccentBrush { get; }
    public Brush Red { get; }
    public Brush Amber { get; }
    public Brush Green { get; }
    public Brush Down { get; }
    public Brush Up { get; }
    public Brush Track { get; }
    public Brush OverlayUnit { get; }
    public Brush OverlayHover { get; }

    public Pen BorderPen { get; }
    public Pen AccentPen { get; }
    public Pen DownPen { get; }
    public Pen UpPen { get; }

    public Font Tiny { get; }
    public Font Small { get; }
    public Font Big { get; }
    public Font Heading { get; }
    public Font PrayerTime { get; }
    public Font Timer { get; }
    public Font Arrow { get; }
    public Font Icon { get; }
    public Font OverlayValue { get; }
    public Font OverlayUnitFont { get; }
    public Font OverlayArrow { get; }
    public Font Dialog { get; }

    public StringFormat Center { get; }
    public StringFormat Left { get; }
    public StringFormat Right { get; }

    public static Theme FromSystem()
    {
        bool light = ReadDword(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme") == 1;
        var accent = Rgb(96, 205, 255);
        if (ReadDword(@"Software\Microsoft\Windows\DWM", "AccentColor") is { } value)
        {
            // Reyestrdagi tartib: ABGR
            accent = Rgb(value & 0xFF, (value >> 8) & 0xFF, (value >> 16) & 0xFF);
        }

        return new Theme(light, Readable(accent, light));
    }

    // To'q aksent qorong'i fonda (yoki och aksent yorug' fonda) o'qilishi uchun moslanadi
    private static Color Readable(Color color, bool light)
    {
        var target = light ? Color.Black : Color.White;
        for (int step = 0; step < 10; step++)
        {
            double luminance = Luminance(color);
            if (light ? luminance <= 140 : luminance >= 140) break;
            color = Rgb(Mix(color.R, target.R), Mix(color.G, target.G), Mix(color.B, target.B));
        }

        return color;
    }

    private static int Mix(int from, int to) => from + (to - from) / 6;

    private static double Luminance(Color c) => 0.299 * c.R + 0.587 * c.G + 0.114 * c.B;

    public void Dispose() => _owned.ForEach(o => o.Dispose());

    private static int? ReadDword(string path, string name)
    {
        using var key = Registry.CurrentUser.OpenSubKey(path);
        return key?.GetValue(name) as int?;
    }

    private static Color Rgb(int r, int g, int b) => Color.FromArgb(r, g, b);

    private Color Pick(Color light, Color dark) => Light ? light : dark;

    private SolidBrush Brush(Color color) => Own(new SolidBrush(color));

    // Piksel birligida: masshtab Graphics transformatsiyasi orqali beriladi
    private Font Font(string family, float points, FontStyle style = FontStyle.Regular) =>
        Own(new Font(family, points * 96f / 72f, style, GraphicsUnit.Pixel));

    private T Own<T>(T item)
        where T : IDisposable
    {
        _owned.Add(item);
        return item;
    }
}
