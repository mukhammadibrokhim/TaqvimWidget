using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TaqvimWidget.Core.Notes;

namespace TaqvimWidget.App.UI;

internal sealed class NotesDialog : ThemedDialog
{
    private readonly NoteStore _store;
    private readonly DateTime _date;
    private readonly ListBox _list;
    private readonly TextBox _text;
    private readonly TextBox _time;
    private IReadOnlyList<Note> _notes = Array.Empty<Note>();

    public NotesDialog(Theme theme, NoteStore store, DateTime date)
        : base(theme, $"Eslatmalar - {date:dd.MM.yyyy}", 380, 340)
    {
        _store = store;
        _date = date;

        _list = Add<ListBox>(this, 12, 12, 356, 170);
        _list.BorderStyle = BorderStyle.FixedSingle;
        Add<Label>(this, 12, 192, 356, 20, "Yangi eslatma:");
        _text = Add<TextBox>(this, 12, 214, 356, 26);
        Add<Label>(this, 12, 252, 186, 20, "Vaqt (SS:DD, ixtiyoriy):");
        _time = Add<TextBox>(this, 200, 249, 70, 26);

        var add = AddButton(12, 292, 110, "Qo'shish", primary: true);
        var remove = AddButton(130, 292, 110, "O'chirish");
        var close = AddButton(258, 292, 110, "Yopish");
        add.Click += (_, _) => AddNote();
        remove.Click += (_, _) => RemoveSelected();
        close.DialogResult = DialogResult.Cancel;
        AcceptButton = add;
        CancelButton = close;

        Reload();
    }

    private void Reload()
    {
        _notes = _store.For(_date);
        _list.Items.Clear();
        foreach (var note in _notes)
        {
            _list.Items.Add($"{(string.IsNullOrEmpty(note.Time) ? "--:--" : note.Time)}   {note.Text}");
        }
    }

    private void AddNote()
    {
        string text = _text.Text.Trim();
        if (text.Length == 0) return;

        if (!NoteStore.TryNormalizeTime(_time.Text, out string time))
        {
            MessageBox.Show(this, "Vaqt formati: SS:DD (masalan 14:30)", "Eslatma");
            return;
        }

        _store.Add(_date, time, text);
        _text.Clear();
        _time.Clear();
        Reload();
        _text.Focus();
    }

    private void RemoveSelected()
    {
        int index = _list.SelectedIndex;
        if (index < 0 || index >= _notes.Count) return;
        _store.Remove(_notes[index]);
        Reload();
    }
}
