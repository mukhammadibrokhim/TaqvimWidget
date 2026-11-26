using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace TaqvimWidget.App.Services;

/// <summary>
/// Faqat foydalanuvchi bosganda ishlaydigan tezlik testi (Cloudflare).
/// Ma'lumot kichik bufer orqali oqim sifatida o'tadi, xotirada to'planmaydi.
/// </summary>
internal static class SpeedTest
{
    private const int MB = 1024 * 1024;
    private const int TimeoutMs = 60_000;
    private const double TooFastSeconds = 2.5;

    public static Task<(double Download, double Upload)> RunAsync() => Task.Run(() =>
    {
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

        double download = MeasureAdaptive(Download, 8 * MB, 40 * MB);

        double upload = 0;
        try
        {
            upload = MeasureAdaptive(Upload, 6 * MB, 24 * MB);
        }
        catch (Exception ex) when (ex is WebException or IOException)
        {
            Log.Write("yuborish testi: " + ex.Message);
        }

        return (download, upload);
    });

    /// <returns>Mbit/s. Aloqa tez bo'lsa, aniqlik uchun kattaroq hajm bilan qayta o'lchaydi.</returns>
    private static double MeasureAdaptive(Func<int, double> transfer, int smallSize, int largeSize)
    {
        int size = smallSize;
        double seconds = transfer(size);
        if (seconds < TooFastSeconds)
        {
            size = largeSize;
            seconds = transfer(size);
        }

        return size * 8 / 1e6 / seconds;
    }

    private static double Download(int bytes)
    {
        var request = CreateRequest("https://speed.cloudflare.com/__down?bytes=" + bytes);
        var watch = Stopwatch.StartNew();
        using var response = request.GetResponse();
        using var stream = response.GetResponseStream()!;
        var buffer = new byte[64 * 1024];
        while (stream.Read(buffer, 0, buffer.Length) > 0)
        {
        }

        return watch.Elapsed.TotalSeconds;
    }

    private static double Upload(int bytes)
    {
        var request = CreateRequest("https://speed.cloudflare.com/__up");
        request.Method = "POST";
        request.ContentLength = bytes;
        request.AllowWriteStreamBuffering = false;

        var buffer = new byte[64 * 1024];
        var watch = Stopwatch.StartNew();
        using (var stream = request.GetRequestStream())
        {
            for (int left = bytes; left > 0; left -= buffer.Length) stream.Write(buffer, 0, Math.Min(left, buffer.Length));
        }

        using (request.GetResponse())
        {
        }

        return watch.Elapsed.TotalSeconds;
    }

    private static HttpWebRequest CreateRequest(string url)
    {
        var request = (HttpWebRequest)WebRequest.Create(url);
        request.Timeout = TimeoutMs;
        request.ReadWriteTimeout = TimeoutMs;
        return request;
    }
}
