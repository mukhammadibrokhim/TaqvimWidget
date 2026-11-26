using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TaqvimWidget.Core.Prayer;
using TaqvimWidget.Core.Settings;

namespace TaqvimWidget.App.UI;

internal sealed class SettingsDialog : ThemedDialog
{
    private readonly Panel _panel;
    private readonly List<Action<AppSettings>> _bindings = new();
    private readonly CheckBox _autostart;
    private int _y = 14;

    public SettingsDialog(Theme theme, AppSettings settings, bool autostartEnabled)
        : base(theme, "Sozlamalar", 420, 560)
    {
        _panel = Add<Panel>(this, 0, 0, 420, 496);
        _panel.AutoScroll = true;

        Choice("Tezlik birligi:", new[] { "Mbit/s (tarif tezligi kabi)", "MB/s (fayl yuklash kabi)" },
            settings.Unit == "mbyte" ? 1 : 0, (s, i) => s.Unit = i == 1 ? "mbyte" : "mbit");
        Toggle("Vazifalar panelida tezlik yozuvini ko'rsatish", settings.Overlay, (s, v) => s.Overlay = v);
        _autostart = Check("Windows bilan birga ishga tushsin", autostartEnabled);
        Toggle("Internet uzilsa/tiklansa bildirishnoma", settings.PingNotify, (s, v) => s.PingNotify = v);
        Number("Oylik trafik limiti (GB, 0 = yo'q):", settings.LimitGB, 0, 100000, (s, v) => s.LimitGB = v);

        Caption("Qaysi kartalar ko'rinsin:");
        Toggle("Namoz vaqtlari", settings.Prayer, (s, v) => s.Prayer = v);
        Toggle("Tizim (CPU, RAM, batareya) va tezlik testi", settings.System, (s, v) => s.System = v);
        Toggle("Pomodoro taymer", settings.Timer, (s, v) => s.Timer = v);
        Toggle("Bayramlarni ajratib ko'rsatish", settings.Holidays, (s, v) => s.Holidays = v);
        _y += 10;

        var cities = Cities.All.Select(c => c.Name).ToArray();
        Choice("Shahar:", cities, Math.Max(0, Array.IndexOf(cities, settings.City)), (s, i) => s.City = cities[i]);
        Number("Bomdod burchagi (gradus):", settings.Fajr, 10, 20, (s, v) => s.Fajr = v, decimals: 1);
        Number("Xufton burchagi (gradus):", settings.Isha, 10, 20, (s, v) => s.Isha = v, decimals: 1);
        Choice("Asr hisobi:", new[] { "Hanafiy (soya 2 barobar)", "Shofe'iy (soya 1 barobar)" },
            settings.Asr == 1 ? 1 : 0, (s, i) => s.Asr = i == 1 ? 1 : 2);

        Toggle("Namoz bildirishnomalari yoqilgan", settings.PrayerNotify, (s, v) => s.PrayerNotify = v);
        PrayerSelection(settings.NotifyPrayers());
        Number("Necha daqiqa oldin eslatilsin (0 = yo'q):", settings.PreMin, 0, 120, (s, v) => s.PreMin = (int)v);
        Toggle("Namoz vaqti kirganda ham xabar kelsin", settings.AtTime, (s, v) => s.AtTime = v);

        var test = Add<Button>(_panel, 14, _y, 220, 30, "Bildirishnomani sinash");
        test.FlatStyle = FlatStyle.Flat;
        test.Click += (_, _) => TestNotificationRequested?.Invoke();
        _y += 44;

        Number("Pomodoro: fokus (daqiqa):", settings.FocusMin, 1, 180, (s, v) => s.FocusMin = (int)v);
        Number("Pomodoro: dam olish (daqiqa):", settings.BreakMin, 1, 60, (s, v) => s.BreakMin = (int)v);

        var note = Add<Label>(_panel, 14, _y + 4, 380, 56,
            "Namoz vaqtlari islom.uz usuliga moslab hisoblanadi (Bomdod va Xufton 15.5 gradus, Hanafiy Asr, " +
            "Shom = quyosh botishi + 4 daqiqa). Hayit kunlari taxminiy.");
        note.ForeColor = theme.GrayColor;

        var save = AddButton(190, 512, 106, "Saqlash", primary: true);
        var cancel = AddButton(304, 512, 102, "Bekor");
        save.DialogResult = DialogResult.OK;
        cancel.DialogResult = DialogResult.Cancel;
        AcceptButton = save;
        CancelButton = cancel;
    }

    public event Action? TestNotificationRequested;

    public bool AutostartSelected => _autostart.Checked;

    public void ApplyTo(AppSettings settings)
    {
        _bindings.ForEach(apply => apply(settings));
        settings.Normalize();
    }

    private void Caption(string text)
    {
        _y += 8;
        Add<Label>(_panel, 14, _y, 380, 22, text);
        _y += 26;
    }

    private CheckBox Check(string text, bool value)
    {
        var box = Add<CheckBox>(_panel, 14, _y, 380, 24, text);
        box.Checked = value;
        _y += 30;
        return box;
    }

    private void Toggle(string text, bool value, Action<AppSettings, bool> apply)
    {
        var box = Check(text, value);
        _bindings.Add(s => apply(s, box.Checked));
    }

    private void Choice(string label, string[] options, int selected, Action<AppSettings, int> apply)
    {
        Add<Label>(_panel, 14, _y + 3, 150, 22, label);
        var combo = Add<WheelSafeComboBox>(_panel, 170, _y, 220, 26);
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.Items.AddRange(options.Cast<object>().ToArray());
        combo.SelectedIndex = selected;
        _bindings.Add(s => apply(s, combo.SelectedIndex));
        _y += 36;
    }

    private void Number(string label, double value, int min, int max, Action<AppSettings, double> apply, int decimals = 0)
    {
        Add<Label>(_panel, 14, _y + 3, 254, 22, label);
        var input = Add<WheelSafeNumericUpDown>(_panel, 270, _y, 120, 26);
        input.DecimalPlaces = decimals;
        input.Increment = decimals > 0 ? 0.5m : 1m;
        input.Minimum = min;
        input.Maximum = max;
        input.Value = Math.Max(min, Math.Min(max, (decimal)value));
        _bindings.Add(s => apply(s, (double)input.Value));
        _y += 36;
    }

    private void PrayerSelection(ICollection<PrayerName> selected)
    {
        Add<Label>(_panel, 14, _y + 3, 380, 22, "Qaysi namozlar uchun xabar kelsin:");
        _y += 26;

        var boxes = new List<(PrayerName Name, CheckBox Box)>();
        int x = 14;
        foreach (var name in new[] { PrayerName.Bomdod, PrayerName.Peshin, PrayerName.Asr, PrayerName.Shom, PrayerName.Xufton })
        {
            var box = Add<CheckBox>(_panel, x, _y, 76, 24, name.ToString());
            box.Checked = selected.Contains(name);
            boxes.Add((name, box));
            x += 76;
        }

        _bindings.Add(s => s.NotifyList = string.Join(",", boxes.Where(b => b.Box.Checked).Select(b => b.Name)));
        _y += 32;
    }

    // Oynani g'ildirak bilan aylantirganda kursor ostidagi qiymat tasodifan o'zgarib ketmasligi kerak
    private sealed class WheelSafeNumericUpDown : NumericUpDown
    {
        protected override void OnMouseWheel(MouseEventArgs e) => WheelGuard.Block(e);
    }

    private sealed class WheelSafeComboBox : ComboBox
    {
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (DroppedDown) base.OnMouseWheel(e);
            else WheelGuard.Block(e);
        }
    }

    private static class WheelGuard
    {
        public static void Block(MouseEventArgs e)
        {
            if (e is HandledMouseEventArgs handled) handled.Handled = true;
        }
    }
}
