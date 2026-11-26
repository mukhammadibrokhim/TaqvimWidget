using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TaqvimWidget.Core.Formatting;
using TaqvimWidget.Core.Storage;

namespace TaqvimWidget.Core.Notes;

public sealed class Note
{
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool Fired { get; set; }
}

public sealed class NoteStore
{
    private readonly List<Note> _notes;
    private readonly string? _path;
    private readonly Action<string>? _log;
    private HashSet<string> _dates = new();

    public NoteStore(string? path = null, Action<string>? log = null)
    {
        _path = path;
        _log = log;
        _notes = path is null ? new List<Note>() : JsonFile.Read<List<Note>>(path, log) ?? new List<Note>();
        _notes.RemoveAll(n => n?.Date is null || n.Text is null);
        Reindex();
    }

    public bool HasNotes(DateTime date) => _dates.Contains(Format.IsoDate(date));

    public IReadOnlyList<Note> For(DateTime date)
    {
        string key = Format.IsoDate(date);
        return _notes.Where(n => n.Date == key).OrderBy(n => n.Time, StringComparer.Ordinal).ToList();
    }

    /// <summary>"9:05" yoki "09.05" ko'rinishidagi vaqtni "09:05" ga keltiradi. Bo'sh matn ruxsat etiladi.</summary>
    public static bool TryNormalizeTime(string input, out string time)
    {
        time = string.Empty;
        input = input.Trim();
        if (input.Length == 0) return true;

        var match = Regex.Match(input, @"^([01]?\d|2[0-3])[:.]([0-5]\d)$");
        if (!match.Success) return false;

        time = $"{int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture):00}:{match.Groups[2].Value}";
        return true;
    }

    public void Add(DateTime date, string time, string text)
    {
        _notes.Add(new Note { Date = Format.IsoDate(date), Time = time, Text = text });
        Commit();
    }

    public void Remove(Note note)
    {
        if (_notes.Remove(note)) Commit();
    }

    /// <summary>Bugungi, vaqti kelgan va hali eslatilmagan yozuvlar.</summary>
    public IReadOnlyList<Notification> TakeDue(DateTime now)
    {
        string today = Format.IsoDate(now);
        string clock = Format.Clock(now);
        var due = _notes
            .Where(n => !n.Fired && n.Date == today && !string.IsNullOrEmpty(n.Time) && string.CompareOrdinal(n.Time, clock) <= 0)
            .ToList();
        if (due.Count == 0) return Array.Empty<Notification>();

        due.ForEach(n => n.Fired = true);
        Commit();
        return due.Select(n => new Notification("Eslatma " + n.Time, n.Text)).ToList();
    }

    private void Commit()
    {
        Reindex();
        if (_path is not null) JsonFile.Write(_path, _notes, _log);
    }

    private void Reindex() => _dates = new HashSet<string>(_notes.Select(n => n.Date));
}
