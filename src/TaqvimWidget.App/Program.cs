using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using TaqvimWidget.App.Services;
using TaqvimWidget.App.UI;

namespace TaqvimWidget.App;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        if (HasFlag(args, "/uninstall"))
        {
            Uninstaller.Run();
            return 0;
        }

        // Ishlab chiqish uchun: widgetni PNG rasmga chizib chiqadi (README skrinshotlari)
        string? snapshot = args.SkipWhile(a => a != "/snapshot").Skip(1).FirstOrDefault();

        using var mutex = new Mutex(true, @"Local\TaqvimWidget", out bool isFirstInstance);
        if (!isFirstInstance && snapshot is null) return 0;

        // Skrinshot rejimi foydalanuvchining shaxsiy ma'lumotlarini ko'rsatmaydi va o'zgartirmaydi
        if (snapshot is null) AppPaths.MigrateLegacyData();
        else AppPaths.Root = Path.Combine(Path.GetTempPath(), "taqvim-snapshot-" + Guid.NewGuid().ToString("N"));

        Application.ThreadException += (_, e) => Log.Write("ui: " + e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Log.Write("fatal: " + e.ExceptionObject);

        // Fon dasturi boshqa ilovalarga xalaqit bermasligi kerak
        using (var process = Process.GetCurrentProcess()) process.PriorityClass = ProcessPriorityClass.BelowNormal;

        using var context = new WidgetContext(interactive: snapshot is null);
        if (snapshot is not null)
        {
            using var image = context.RenderSnapshot();
            image.Save(snapshot, ImageFormat.Png);
            return 0;
        }

        if (HasFlag(args, "/show")) context.ShowFlyout();
        Application.Run(context);
        return 0;
    }

    private static bool HasFlag(string[] args, string flag) => args.Contains(flag, StringComparer.OrdinalIgnoreCase);
}
