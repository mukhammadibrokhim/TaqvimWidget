using System;
using System.Collections.Generic;
using System.Linq;
using TaqvimWidget.Core.Formatting;
using TaqvimWidget.Core.Storage;

namespace TaqvimWidget.Core.Network;

/// <summary>Kunlik trafik hisobi. Fayl formati: {"yyyy-MM-dd": [qabul, yuborish]}.</summary>
public sealed class TrafficLedger
{
    private const int KeepDays = 120;
    private readonly Dictionary<string, double[]> _days;
    private bool _dirty;

    public TrafficLedger(Dictionary<string, double[]>? days = null)
    {
        _days = new Dictionary<string, double[]>();
        foreach (var pair in days ?? new Dictionary<string, double[]>())
        {
            if (pair.Value is { Length: 2 }) _days[pair.Key] = pair.Value;
        }
    }

    public static TrafficLedger Load(string path, Action<string>? log = null) =>
        new(JsonFile.Read<Dictionary<string, double[]>>(path, log));

    public void Add(DateTime day, long received, long sent)
    {
        if (received == 0 && sent == 0) return;

        string key = Format.IsoDate(day);
        if (!_days.TryGetValue(key, out var value)) _days[key] = value = new double[2];
        value[0] += received;
        value[1] += sent;
        _dirty = true;
    }

    public (double Received, double Sent) Day(DateTime day) =>
        _days.TryGetValue(Format.IsoDate(day), out var v) ? (v[0], v[1]) : (0, 0);

    public double MonthTotal(DateTime day)
    {
        string prefix = Format.IsoDate(day).Substring(0, 8);
        return _days.Where(p => p.Key.StartsWith(prefix, StringComparison.Ordinal)).Sum(p => p.Value[0] + p.Value[1]);
    }

    /// <summary>Faqat o'zgarish bo'lsa yozadi (diskka ortiqcha murojaat qilmaslik uchun).</summary>
    public void Save(string path, DateTime today, Action<string>? log = null)
    {
        if (!_dirty) return;

        string cutoff = Format.IsoDate(today.AddDays(-KeepDays));
        foreach (var key in _days.Keys.Where(k => string.CompareOrdinal(k, cutoff) < 0).ToList()) _days.Remove(key);

        JsonFile.Write(path, _days, log);
        _dirty = false;
    }
}
