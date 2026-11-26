using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TaqvimWidget.Core.Network;

namespace TaqvimWidget.App.Services;

internal sealed class PingMonitor : IDisposable
{
    private const int TimeoutMs = 1500;
    private static readonly string[] Hosts = { "1.1.1.1", "8.8.8.8" };

    private readonly Ping _ping = new();
    private bool _busy;
    private bool _disposed;

    public ConnectivityTracker State { get; } = new();

    public async Task<ConnectivityChange> CheckAsync()
    {
        if (_busy || _disposed) return ConnectivityChange.None;
        _busy = true;
        try
        {
            // Birinchi manzil javob bermasa, keyingi urinish boshqasiga yuboriladi
            var reply = await _ping.SendPingAsync(Hosts[State.Failures % Hosts.Length], TimeoutMs);
            return State.Report(reply.Status == IPStatus.Success ? reply.RoundtripTime : null);
        }
        catch (Exception ex) when (ex is PingException or ObjectDisposedException or InvalidOperationException)
        {
            return State.Report(null);
        }
        finally
        {
            _busy = false;
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _ping.Dispose();
    }
}
