#region Using

using System.Runtime.InteropServices;

#endregion

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
namespace WinApi.ComBaseApi.COM
{
    /// <summary>
    /// implements IMMDeviceEnumerator
    /// </summary>
    //[ComImport]
    //[Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    //internal class MMDeviceEnumeratorComObject
    //{
    //}

    internal static class MMDeviceEnumeratorComObject
    {
        [DllImport("ole32.dll")]
        private static extern int CoCreateInstance(
                in Guid rclsid,
                nint pUnkOuter,
                int dwClsContext,
                in Guid riid,
                [MarshalAs(UnmanagedType.Interface)]
                out object ppv);
        internal static IMMDeviceEnumerator Create()
        {
            var mmDeviceEnumerator = new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E");

            const int CLSCTX_INPROC_SERVER = 0x1;
            int hr = CoCreateInstance(
                mmDeviceEnumerator,
                0,
                CLSCTX_INPROC_SERVER,
                typeof(IMMDeviceEnumerator).GUID,
                out object objectDeviceEnumerator);

            Marshal.ThrowExceptionForHR(hr);

            return (IMMDeviceEnumerator)objectDeviceEnumerator;
        }
    }
}