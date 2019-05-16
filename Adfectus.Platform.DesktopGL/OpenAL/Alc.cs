#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace Adfectus.OpenAL
{
    public class Alc
    {
        #region Internal Delegates

        private delegate IntPtr _CreateContextInternal(IntPtr device, IntPtr attrlist);

        public delegate bool MakeContextCurrentInternal(IntPtr context);

        public delegate void ProcessContextInternal(IntPtr context);

        public delegate void SuspendContextInternal(IntPtr context);

        public delegate void DestroyContextInternal(IntPtr context);

        public delegate IntPtr GetCurrentContextInternal();

        public delegate IntPtr GetContextsDeviceInternal(IntPtr context);

        public delegate IntPtr OpenDeviceInternal(string deviceName);

        public delegate bool CloseDeviceInternal(IntPtr device);

        public delegate int GetErrorInternal(IntPtr device);

        public delegate bool IsExtensionPresentInternal(IntPtr device, string extname);

        public delegate IntPtr GetProcAddressInternal(IntPtr device, string funcname);

        public delegate int GetEnumValueInternal(IntPtr device, string enumname);

        private delegate IntPtr _GetStringInternal(IntPtr device, int param);

        public delegate void GetIntegervInternal(IntPtr device, int param, int size, IntPtr data);

        public delegate IntPtr CaptureOpenDeviceInternal(string devicename, uint frequency, int format, int buffersize);

        public delegate bool CaptureCloseDeviceInternal(IntPtr device);

        public delegate void CaptureStartInternal(IntPtr device);

        public delegate void CaptureStopInternal(IntPtr device);

        public unsafe delegate void CaptureSamplesInternal(IntPtr device, void* buffer, int samples);

        #endregion

        #region Function Handles

        private static _CreateContextInternal _CreateContext;
        public static MakeContextCurrentInternal MakeContextCurrent;
        public static ProcessContextInternal ProcessContext;
        public static SuspendContextInternal SuspendContext;
        public static DestroyContextInternal DestroyContext;
        public static GetCurrentContextInternal GetCurrentContext;
        public static GetContextsDeviceInternal GetContextsDevice;
        public static OpenDeviceInternal OpenDevice;
        public static CloseDeviceInternal CloseDevice;
        public static GetErrorInternal GetError;
        public static IsExtensionPresentInternal IsExtensionPresent;
        public static GetProcAddressInternal GetProcAddress;
        public static GetEnumValueInternal GetEnumValue;
        private static _GetStringInternal _GetString;
        public static GetIntegervInternal GetIntegerv;
        public static CaptureOpenDeviceInternal CaptureOpenDevice;
        public static CaptureCloseDeviceInternal CaptureCloseDevice;
        public static CaptureStartInternal CaptureStart;
        public static CaptureStopInternal CaptureStop;
        public static CaptureSamplesInternal CaptureSamples;

        #endregion

        /// <summary>
        /// Initialize the library.
        /// </summary>
        /// <param name="lib">A pointer to the loaded library using the NativeLibrary class.</param>
        /// <returns>Whether initialization was successful.</returns>
        public static int Init(IntPtr lib)
        {
            GetFuncPointer(lib, "alcCreateContext", ref _CreateContext);
            GetFuncPointer(lib, "alcMakeContextCurrent", ref MakeContextCurrent);
            GetFuncPointer(lib, "alcProcessContext", ref ProcessContext);
            GetFuncPointer(lib, "alcSuspendContext", ref SuspendContext);
            GetFuncPointer(lib, "alcDestroyContext", ref DestroyContext);
            GetFuncPointer(lib, "alcGetCurrentContext", ref GetCurrentContext);
            GetFuncPointer(lib, "alcGetContextsDevice", ref GetContextsDevice);
            GetFuncPointer(lib, "alcOpenDevice", ref OpenDevice);
            GetFuncPointer(lib, "alcCloseDevice", ref CloseDevice);
            GetFuncPointer(lib, "alcGetError", ref GetError);
            GetFuncPointer(lib, "alcIsExtensionPresent", ref IsExtensionPresent);
            GetFuncPointer(lib, "alcGetProcAddress", ref GetProcAddress);
            GetFuncPointer(lib, "alcGetEnumValue", ref GetEnumValue);
            GetFuncPointer(lib, "alcGetString", ref _GetString);
            GetFuncPointer(lib, "alcGetIntegerv", ref GetIntegerv);
            GetFuncPointer(lib, "alcCaptureOpenDevice", ref CaptureOpenDevice);
            GetFuncPointer(lib, "alcCaptureCloseDevice", ref CaptureCloseDevice);
            GetFuncPointer(lib, "alcCaptureStart", ref CaptureStart);
            GetFuncPointer(lib, "alcCaptureStop", ref CaptureStop);
            GetFuncPointer(lib, "alcCaptureSamples", ref CaptureSamples);

            return 1;
        }

        /// <summary>
        /// Load a function from the library.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate type to load the function as.</typeparam>
        /// <param name="lib">Pointer to the library.</param>
        /// <param name="name">The name of the function.</param>
        /// <param name="funcDel">The delegate variable which to load the function into.</param>
        private static void GetFuncPointer<TDelegate>(IntPtr lib, string name, ref TDelegate funcDel)
        {
            bool success = NativeLibrary.TryGetExport(lib, name, out IntPtr func);
            if (success) funcDel = Marshal.GetDelegateForFunctionPointer<TDelegate>(func);
        }

        #region Enum

        public const int False = 0;
        public const int True = 1;
        public const int Frequency = 0x1007;
        public const int Refresh = 0x1008;
        public const int Sync = 0x1009;
        public const int MonoSources = 0x1010;
        public const int StereoSources = 0x1011;
        public const int NoError = False;
        public const int InvalidDevice = 0xA001;
        public const int InvalidContext = 0xA002;
        public const int InvalidEnum = 0xA003;
        public const int InvalidValue = 0xA004;
        public const int OutOfMemory = 0xA005;
        public const int DefaultDeviceSpecifier = 0x1004;
        public const int DeviceSpecifier = 0x1005;
        public const int Extensions = 0x1006;
        public const int MajorVersion = 0x1000;
        public const int MinorVersion = 0x1001;
        public const int AttributesSize = 0x1002;
        public const int AllAttributes = 0x1003;
        public const int DefaultAllDevicesSpecifier = 0x1012;
        public const int AllDevicesSpecifier = 0x1013;
        public const int CaptureDeviceSpecifier = 0x310;
        public const int CaptureDefaultDeviceSpecifier = 0x311;
        public const int EnumCaptureSamples = 0x312;

        #endregion

        #region Context Management Functions

        public static string GetString(IntPtr device, int param)
        {
            return Marshal.PtrToStringAnsi(_GetString(device, param));
        }

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

        #endregion
    }
}