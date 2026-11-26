using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace TaqvimWidget.App.Services;

/// <summary>
/// Internetga chiqadigan adapterlarning bayt hisoblagichlarini o'qiydi (passiv: trafik yaratmaydi).
/// Adapterlar ro'yxati qimmat so'rov, shuning uchun faqat tarmoq o'zgarganda yangilanadi.
/// </summary>
internal sealed class NetworkSampler : IDisposable
{
    private const int FallbackRefreshTicks = 60;

    private NetworkInterface[] _adapters = Array.Empty<NetworkInterface>();
    private string _adapterKey = string.Empty;
    private int _ticksSinceRefresh = FallbackRefreshTicks;
    private volatile bool _stale = true;

    public NetworkSampler() => NetworkChange.NetworkAddressChanged += OnNetworkChanged;

    public void Dispose() => NetworkChange.NetworkAddressChanged -= OnNetworkChanged;

    /// <returns>Jami qabul/yuborilgan baytlar va adapterlar to'plami o'zgargan-o'zgarmagani.</returns>
    public (long Received, long Sent, bool AdaptersChanged) Read()
    {
        bool changed = false;
        if (_stale || ++_ticksSinceRefresh >= FallbackRefreshTicks) changed = Refresh();

        long received = 0;
        long sent = 0;
        foreach (var adapter in _adapters)
        {
            try
            {
                var statistics = adapter.GetIPv4Statistics();
                received += statistics.BytesReceived;
                sent += statistics.BytesSent;
            }
            catch (NetworkInformationException)
            {
                _stale = true; // adapter uzilgan
            }
        }

        return (received, sent, changed);
    }

    private void OnNetworkChanged(object? sender, EventArgs e) => _stale = true;

    private bool Refresh()
    {
        _stale = false;
        _ticksSinceRefresh = 0;
        try
        {
            _adapters = NetworkInterface.GetAllNetworkInterfaces().Where(IsInternetAdapter).ToArray();
        }
        catch (NetworkInformationException ex)
        {
            Log.Write("adapterlar: " + ex.Message);
            _adapters = Array.Empty<NetworkInterface>();
        }

        string key = string.Join(",", _adapters.Select(a => a.Id));
        if (key == _adapterKey) return false;
        _adapterKey = key;
        return true;
    }

    private static bool IsInternetAdapter(NetworkInterface adapter) =>
        adapter.OperationalStatus == OperationalStatus.Up
        && adapter.NetworkInterfaceType is not (NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
        && adapter.GetIPProperties().GatewayAddresses.Any(g =>
            g.Address.AddressFamily == AddressFamily.InterNetwork && !g.Address.Equals(System.Net.IPAddress.Any));
}
