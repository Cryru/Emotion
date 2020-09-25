#region Using

using System;
using System.Runtime.InteropServices;

#endregion

// ReSharper disable UnusedMember.Global

// ReSharper disable once CheckNamespace
namespace OpenAL
{
    public class Alc
    {
        #region Enum

        public const int FALSE = 0;
        public const int TRUE = 1;
        public const int FREQUENCY = 0x1007;
        public const int REFRESH = 0x1008;
        public const int SYNC = 0x1009;
        public const int MONO_SOURCES = 0x1010;
        public const int STEREO_SOURCES = 0x1011;
        public const int NO_ERROR = FALSE;
        public const int INVALID_DEVICE = 0xA001;
        public const int INVALID_CONTEXT = 0xA002;
        public const int INVALID_ENUM = 0xA003;
        public const int INVALID_VALUE = 0xA004;
        public const int OUT_OF_MEMORY = 0xA005;
        public const int DEFAULT_DEVICE_SPECIFIER = 0x1004;
        public const int DEVICE_SPECIFIER = 0x1005;
        public const int EXTENSIONS = 0x1006;
        public const int MAJOR_VERSION = 0x1000;
        public const int MINOR_VERSION = 0x1001;
        public const int ATTRIBUTES_SIZE = 0x1002;
        public const int ALL_ATTRIBUTES = 0x1003;
        public const int DEFAULT_ALL_DEVICES_SPECIFIER = 0x1012;
        public const int ALL_DEVICES_SPECIFIER = 0x1013;
        public const int CAPTURE_DEVICE_SPECIFIER = 0x310;
        public const int CAPTURE_DEFAULT_DEVICE_SPECIFIER = 0x311;
        public const int ENUM_CAPTURE_SAMPLES = 0x312;

        #endregion

        #region Context Management Functions

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcCreateContext")]
        private static extern IntPtr _CreateContext(IntPtr device, IntPtr attrlist);

        public static IntPtr CreateContext(IntPtr device, int[] attrList)
        {
            unsafe
            {
                fixed (int* arrayPtr = attrList)
                {
                    return _CreateContext(device, new IntPtr(arrayPtr));
                }
            }
        }

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcMakeContextCurrent")]
        public static extern bool MakeContextCurrent(IntPtr context);

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcProcessContext")]
        public static extern void ProcessContext(IntPtr context);

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcSuspendContext")]
        public static extern void SuspendContext(IntPtr context);

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcDestroyContext")]
        public static extern void DestroyContext(IntPtr context);

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcGetCurrentContext")]
        public static extern IntPtr GetCurrentContext();

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcGetContextsDevice")]
        public static extern IntPtr GetContextsDevice(IntPtr context);

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcOpenDevice")]
        public static extern IntPtr OpenDevice(string deviceName);

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcCloseDevice")]
        public static extern bool CloseDevice(IntPtr device);

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcGetError")]
        public static extern int GetError(IntPtr device);

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcIsExtensionPresent")]
        public static extern bool IsExtensionPresent(IntPtr device, string extname);

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcGetProcAddress")]
        public static extern IntPtr GetProcAddress(IntPtr device, string funcname);

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcGetEnumValue")]
        public static extern int GetEnumValue(IntPtr device, string enumname);

        #endregion

        #region Query Functions

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcGetString")]
        private static extern IntPtr _GetString(IntPtr device, int param);

        public static string GetString(IntPtr device, int param)
        {
            return Marshal.PtrToStringAnsi(_GetString(device, param));
        }

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcGetIntegerv")]
        public static extern void GetIntegerv(IntPtr device, int param, int size, IntPtr data);

        #endregion

        #region Capture Functions

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcCaptureOpenDevice")]
        public static extern IntPtr CaptureOpenDevice(string devicename, uint frequency, int format, int buffersize);

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcCaptureCloseDevice")]
        public static extern bool CaptureCloseDevice(IntPtr device);

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcCaptureStart")]
        public static extern void CaptureStart(IntPtr device);

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcCaptureStop")]
        public static extern void CaptureStop(IntPtr device);

        [DllImport(Al.OPEN_AL_DLL, EntryPoint = "alcCaptureSamples")]
        public static extern unsafe void CaptureSamples(IntPtr device, void* buffer, int samples);

        #endregion
    }
}