namespace TaqvimWidget.Core.Network;

public enum ConnectivityChange
{
    None,
    Lost,
    Restored,
}

/// <summary>Ping natijalaridan aloqa holatini aniqlaydi (bitta yo'qolgan paket uzilish hisoblanmaydi).</summary>
public sealed class ConnectivityTracker
{
    private const int FailuresBeforeOffline = 2;

    public bool Online { get; private set; } = true;
    public int Failures { get; private set; }

    /// <summary>Millisekundlarda; hali o'lchanmagan yoki oflayn bo'lsa null.</summary>
    public long? Latency { get; private set; }

    public ConnectivityChange Report(long? roundtrip)
    {
        if (roundtrip is null)
        {
            Failures++;
            if (!Online || Failures < FailuresBeforeOffline) return ConnectivityChange.None;
            Online = false;
            Latency = null;
            return ConnectivityChange.Lost;
        }

        Failures = 0;
        Latency = roundtrip;
        if (Online) return ConnectivityChange.None;
        Online = true;
        return ConnectivityChange.Restored;
    }
}
