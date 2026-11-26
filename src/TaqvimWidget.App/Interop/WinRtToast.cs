using System;
using System.Runtime.InteropServices;
using System.Security;

namespace TaqvimWidget.App.Interop;

/// <summary>
/// WinRT bildirishnoma API'siga tashqi paketlarsiz murojaat. Interfeyslarda faqat kerakli
/// metodlar e'lon qilingan; ulardan oldingi metodlar tartibi saqlanishi shart.
/// </summary>
internal static class WinRtToast
{
    public static void Show(string appUserModelId, string title, string message)
    {
        object document = ActivateInstance("Windows.Data.Xml.Dom.XmlDocument");
        ((IXmlDocumentIO)document).LoadXml(
            "<toast><visual><binding template=\"ToastGeneric\">" +
            $"<text>{SecurityElement.Escape(title)}</text><text>{SecurityElement.Escape(message)}</text>" +
            "</binding></visual></toast>");

        var factory = (IToastNotificationFactory)Factory("Windows.UI.Notifications.ToastNotification", typeof(IToastNotificationFactory).GUID);
        var manager = (IToastNotificationManagerStatics)Factory("Windows.UI.Notifications.ToastNotificationManager", typeof(IToastNotificationManagerStatics).GUID);

        IntPtr unknown = Marshal.GetIUnknownForObject(document);
        IntPtr xml = IntPtr.Zero;
        IntPtr toast = IntPtr.Zero;
        try
        {
            var xmlDocumentId = new Guid("F7F3A506-1E87-42D6-BCFB-B8C809FA5494");
            Marshal.ThrowExceptionForHR(Marshal.QueryInterface(unknown, ref xmlDocumentId, out xml));
            toast = factory.CreateToastNotification(xml);
            manager.CreateToastNotifierWithId(appUserModelId).Show(toast);
        }
        finally
        {
            if (toast != IntPtr.Zero) Marshal.Release(toast);
            if (xml != IntPtr.Zero) Marshal.Release(xml);
            Marshal.Release(unknown);
        }
    }

    private static object ActivateInstance(string className)
    {
        using var name = new HString(className);
        Marshal.ThrowExceptionForHR(RoActivateInstance(name.Handle, out object instance));
        return instance;
    }

    private static object Factory(string className, Guid interfaceId)
    {
        using var name = new HString(className);
        Marshal.ThrowExceptionForHR(RoGetActivationFactory(name.Handle, ref interfaceId, out object factory));
        return factory;
    }

    private sealed class HString : IDisposable
    {
        public HString(string value) => Marshal.ThrowExceptionForHR(WindowsCreateString(value, value.Length, out _handle));

        private IntPtr _handle;

        public IntPtr Handle => _handle;

        public void Dispose()
        {
            WindowsDeleteString(_handle);
            _handle = IntPtr.Zero;
        }
    }

    [DllImport("combase.dll", CharSet = CharSet.Unicode)]
    private static extern int WindowsCreateString(string source, int length, out IntPtr handle);

    [DllImport("combase.dll")]
    private static extern int WindowsDeleteString(IntPtr handle);

    [DllImport("combase.dll")]
    private static extern int RoActivateInstance(IntPtr className, [MarshalAs(UnmanagedType.IInspectable)] out object instance);

    [DllImport("combase.dll")]
    private static extern int RoGetActivationFactory(IntPtr className, ref Guid interfaceId, [MarshalAs(UnmanagedType.IInspectable)] out object factory);

    [ComImport, Guid("6CD0E74E-EE65-4489-9EBF-CA43E87BA637"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    private interface IXmlDocumentIO
    {
        void LoadXml([MarshalAs(UnmanagedType.HString)] string xml);
    }

    [ComImport, Guid("04124B20-82C6-4229-B109-FD9ED4662B53"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    private interface IToastNotificationFactory
    {
        IntPtr CreateToastNotification(IntPtr xmlDocument);
    }

    [ComImport, Guid("50AC103F-D235-4598-BBEF-98FE4D1A3AD4"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    private interface IToastNotificationManagerStatics
    {
        IToastNotifier CreateToastNotifier();
        IToastNotifier CreateToastNotifierWithId([MarshalAs(UnmanagedType.HString)] string applicationId);
    }

    [ComImport, Guid("75927B93-03F3-41EC-91D3-6E5BAC1B38E7"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    private interface IToastNotifier
    {
        void Show(IntPtr notification);
    }
}
