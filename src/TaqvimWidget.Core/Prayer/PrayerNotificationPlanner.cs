using System;
using System.Collections.Generic;
using TaqvimWidget.Core.Formatting;

namespace TaqvimWidget.Core.Prayer;

/// <summary>Qaysi namoz bildirishnomasi hozir yuborilishi kerakligini aniqlaydi (har biri bir marta).</summary>
public sealed class PrayerNotificationPlanner
{
    private static readonly TimeSpan AtTimeWindow = TimeSpan.FromMinutes(5);
    private readonly HashSet<string> _sent = new();

    public void Reset() => _sent.Clear();

    public IEnumerable<Notification> Due(DateTime now, PrayerSchedule schedule, ICollection<PrayerName> enabled, int minutesBefore, bool atTime)
    {
        var today = schedule.For(now);
        var result = new List<Notification>();

        foreach (var name in enabled)
        {
            if (name == PrayerName.Quyosh) continue;

            var time = today[name];
            string day = now.ToString("yyyyMMdd");

            if (minutesBefore > 0 && now < time && now >= time.AddMinutes(-minutesBefore) && _sent.Add(day + "pre" + name))
            {
                int left = (int)Math.Ceiling((time - now).TotalMinutes);
                result.Add(new Notification(
                    $"{name} namoziga {left} daqiqa qoldi",
                    $"{name} {Format.Clock(time)} - {schedule.City.Name}"));
            }

            if (atTime && now >= time && now - time < AtTimeWindow && _sent.Add(day + "at" + name))
            {
                result.Add(new Notification($"{name} vaqti kirdi", $"{schedule.City.Name} - {Format.Clock(time)}"));
            }
        }

        return result;
    }
}
