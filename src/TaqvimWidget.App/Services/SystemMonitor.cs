using System;
using System.Windows.Forms;
using TaqvimWidget.App.Interop;

namespace TaqvimWidget.App.Services;

/// <summary>CPU, RAM va batareya holati. Faqat widget ochiq turganda so'raladi.</summary>
internal sealed class SystemMonitor
{
    private (long Idle, long Kernel, long User)? _previous;

    public int Cpu { get; private set; }
    public int Memory { get; private set; }

    /// <summary>Batareya bo'lmasa null.</summary>
    public int? Battery { get; private set; }

    public bool PluggedIn { get; private set; }

    public void Restart() => _previous = null;

    public void Sample()
    {
        if (NativeMethods.GetSystemTimes(out long idle, out long kernel, out long user))
        {
            if (_previous is { } p)
            {
                long total = kernel - p.Kernel + (user - p.User);
                if (total > 0) Cpu = (int)Math.Round(100.0 * (total - (idle - p.Idle)) / total);
            }

            _previous = (idle, kernel, user);
        }

        var memory = NativeMethods.MemoryStatus.Create();
        if (NativeMethods.GlobalMemoryStatusEx(ref memory)) Memory = (int)memory.MemoryLoad;

        var power = SystemInformation.PowerStatus;
        bool noBattery = (power.BatteryChargeStatus & BatteryChargeStatus.NoSystemBattery) != 0;
        Battery = noBattery ? null : (int)Math.Round(Math.Min(1f, power.BatteryLifePercent) * 100);
        PluggedIn = power.PowerLineStatus == PowerLineStatus.Online;
    }
}
