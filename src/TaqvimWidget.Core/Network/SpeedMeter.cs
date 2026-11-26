using System;

namespace TaqvimWidget.Core.Network;

/// <summary>Tarmoq adapterining umumiy bayt hisoblagichlaridan tezlikni hisoblaydi.</summary>
public sealed class SpeedMeter
{
    public const int HistoryLength = 60;

    private long _lastReceived = -1;
    private long _lastSent = -1;
    private double _lastSeconds;
    private int _head;

    private readonly double[] _downHistory = new double[HistoryLength];
    private readonly double[] _upHistory = new double[HistoryLength];

    public double Download { get; private set; }
    public double Upload { get; private set; }
    public int Count { get; private set; }

    /// <summary>index: 0 - eng eski o'lchov.</summary>
    public (double Down, double Up) History(int index)
    {
        int i = (_head - Count + index + HistoryLength) % HistoryLength;
        return (_downHistory[i], _upHistory[i]);
    }

    /// <summary>Adapterlar to'plami o'zgarganda chaqiriladi: keyingi o'lchov soxta sakrash bermaydi.</summary>
    public void Reset() => _lastReceived = -1;

    /// <returns>Oldingi o'lchovdan beri o'tgan baytlar.</returns>
    public (long Received, long Sent) Sample(long totalReceived, long totalSent, double seconds)
    {
        long received = 0;
        long sent = 0;
        double elapsed = seconds - _lastSeconds;

        if (_lastReceived >= 0 && elapsed > 0.2)
        {
            received = Math.Max(0, totalReceived - _lastReceived);
            sent = Math.Max(0, totalSent - _lastSent);
            Download = received / elapsed;
            Upload = sent / elapsed;
        }
        else
        {
            Download = 0;
            Upload = 0;
        }

        _lastReceived = totalReceived;
        _lastSent = totalSent;
        _lastSeconds = seconds;

        _downHistory[_head] = Download;
        _upHistory[_head] = Upload;
        _head = (_head + 1) % HistoryLength;
        if (Count < HistoryLength) Count++;

        return (received, sent);
    }
}
