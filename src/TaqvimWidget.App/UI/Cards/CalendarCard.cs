using System;
using System.Drawing;
using TaqvimWidget.Core.Calendar;

namespace TaqvimWidget.App.UI.Cards;

internal sealed class CalendarCard : Card
{
    private const float GridLeft = 34;
    private const float GridTop = 64;
    private const float CellWidth = 38;
    private const float CellHeight = 32;

    private static readonly string[] WeekDays = { "Du", "Se", "Ch", "Pa", "Ju", "Sh", "Ya" };

    private int _year = DateTime.Today.Year;
    private int _month = DateTime.Today.Month;
    private (DateTime Day, (DateTime Date, string Name)? Holiday)? _nextHoliday;

    public CalendarCard(WidgetState state, Theme theme, Action<DateTime> openDay)
        : base(state, theme)
    {
        AddHotspot(new RectangleF(236, 8, 34, 30), () => Move(-1));
        AddHotspot(new RectangleF(270, 8, 34, 30), () => Move(1));
        AddHotspot(new RectangleF(12, 8, 220, 30), ShowToday);
        for (int i = 0; i < MonthGrid.Rows * MonthGrid.Columns; i++)
        {
            int index = i;
            AddHotspot(CellArea(index), () => openDay(MonthGrid.Cell(_year, _month, index)));
        }
    }

    public override int Height => 288;
    public override bool Visible => true;

    public void ShowToday()
    {
        _year = DateTime.Today.Year;
        _month = DateTime.Today.Month;
    }

    public void Move(int months)
    {
        var date = new DateTime(_year, _month, 1).AddMonths(months);
        if (date.Year is < 1950 or > 2100) return;
        _year = date.Year;
        _month = date.Month;
    }

    protected override void PaintContent(Graphics g, DateTime now)
    {
        g.Write($"{MonthGrid.MonthName(_month)} {_year}", Theme.Heading, Theme.Text, new RectangleF(12, 8, 220, 30), Theme.Left);
        g.Write("‹", Theme.Arrow, Theme.Gray, new RectangleF(236, 8, 34, 30), Theme.Center);
        g.Write("›", Theme.Arrow, Theme.Gray, new RectangleF(270, 8, 34, 30), Theme.Center);

        g.Write("#", Theme.Tiny, Theme.Dim, new RectangleF(12, 42, 22, 20), Theme.Center);
        for (int column = 0; column < MonthGrid.Columns; column++)
        {
            var brush = column == 6 ? Theme.Red : Theme.Gray;
            g.Write(WeekDays[column], Theme.Tiny, brush, new RectangleF(GridLeft + column * CellWidth, 42, CellWidth, 20), Theme.Center);
        }

        var first = MonthGrid.FirstCell(_year, _month);
        for (int row = 0; row < MonthGrid.Rows; row++)
        {
            int week = MonthGrid.IsoWeek(first.AddDays(row * 7));
            g.Write(week.ToString(), Theme.Tiny, Theme.Dim, new RectangleF(12, GridTop + row * CellHeight, 22, CellHeight), Theme.Center);
        }

        for (int i = 0; i < MonthGrid.Rows * MonthGrid.Columns; i++) PaintDay(g, i, first.AddDays(i), now.Date);

        if (State.Settings.Holidays) PaintNextHoliday(g, now.Date);
    }

    private static RectangleF CellArea(int index) => new(
        GridLeft + index % MonthGrid.Columns * CellWidth,
        GridTop + index / MonthGrid.Columns * CellHeight,
        CellWidth,
        CellHeight);

    private void PaintDay(Graphics g, int index, DateTime date, DateTime today)
    {
        var area = CellArea(index);
        bool isToday = date == today;
        if (isToday) g.FillEllipse(Theme.AccentBrush, area.X + 4, area.Y + 1, 30, 30);

        Brush brush;
        if (isToday) brush = Theme.OnAccentBrush;
        else if (date.Month != _month) brush = Theme.Dim;
        else if (index % MonthGrid.Columns == 6 || (State.Settings.Holidays && State.Holidays.IsHoliday(date))) brush = Theme.Red;
        else brush = Theme.Text;

        g.Write(date.Day.ToString(), Theme.Small, brush, area, Theme.Center);

        if (State.Notes.HasNotes(date))
        {
            g.FillEllipse(isToday ? Theme.OnAccentBrush : Theme.AccentBrush, area.X + 17, area.Y + 25, 4, 4);
        }
    }

    private void PaintNextHoliday(Graphics g, DateTime today)
    {
        if (_nextHoliday?.Day != today) _nextHoliday = (today, State.Holidays.Next(today));
        if (_nextHoliday?.Holiday is not { } holiday) return;

        int days = (holiday.Date - today).Days;
        string text = days == 0
            ? "Bugun: " + holiday.Name
            : $"Keyingi bayram: {holiday.Name} - {holiday.Date.Day}-{MonthGrid.MonthName(holiday.Date.Month).ToLowerInvariant()} ({days} kun)";
        g.Write(text, Theme.Tiny, Theme.Red, 12, 264);
    }
}
