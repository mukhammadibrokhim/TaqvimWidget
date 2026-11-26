using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace TaqvimWidget.Setup;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Contains("/silent", StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                Process.Start(Installer.Install(autostart: true, desktopShortcut: false));
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new SetupForm());
        return 0;
    }
}
