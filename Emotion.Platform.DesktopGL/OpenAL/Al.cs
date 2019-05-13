#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace Adfectus.OpenAL
{
    public class Al
    {
        #region Internal Delegates

        public delegate void EnableInternal(int capability);

        public delegate void DisableInternal(int capability);

        public delegate bool IsEnabledInternal(int capability);

        private delegate IntPtr _GetStringInternal(int param);

        public delegate void GetBooleanvInternal(int param, out bool data);

        public delegate void GetIntegervInternal(int param, out int data);

        public delegate void GetFloatvInternal(int param, out float data);

        public delegate void GetDoublevInternal(int param, out double data);

        public delegate bool GetBooleanInternal(int param);

        public delegate int GetIntegerInternal(int param);

        public delegate float GetFloatInternal(int param);

        public delegate double GetDoubleInternal(int param);

        public delegate int GetErrorInternal();

        public delegate bool IsExtensionPresentInternal(string extname);

        public unsafe delegate void* GetProcAddressInternal(string fname);

        public delegate int GetEnumValueInternal(string ename);

        public delegate void ListenerfInternal(int param, float value);

        public delegate void Listener3fInternal(int param, float value1, float value2, float value3);

        public delegate void ListenerfvInternal(int param, float[] values);

        public delegate void GetListenerfInternal(int param, out float value);

        public delegate void GetListener3fInternal(int param, out float value1, out float value2, out float value3);

        private unsafe delegate void _GetListenerfvInternal(int param, float* values);

        private unsafe delegate void _GenSourcesInternal(int n, uint* sources);

        public delegate void DeleteSourcesInternal(int n, uint[] sources);

        public delegate bool IsSourceInternal(uint sid);

        public delegate void SourcefInternal(uint sid, int param, float value);

        public delegate void Source3fInternal(uint sid, int param, float value1, float value2, float value3);

        public delegate void SourcefvInternal(uint sid, int param, float[] values);

        public delegate void SourceiInternal(uint sid, int param, int value);

        public delegate void Source3iInternal(uint sid, int param, int value1, int value2, int value3);

        public delegate void SourceivInternal(uint sid, int param, int[] values);

        public delegate void GetSourcefInternal(uint sid, int param, out float value);

        public delegate void GetSource3fInternal(uint sid, int param, out float value1, out float value2, out float value3);

        private unsafe delegate void _GetSourcefvInternal(uint sid, int param, float* values);

        public delegate void GetSourceiInternal(uint sid, int param, out int value);

        public delegate void GetSource3iInternal(uint sid, int param, out int value1, out int value2, out int value3);

        private unsafe delegate void _GetSourceivInternal(uint sid, int param, int* values);

        public delegate void SourcePlayvInternal(int ns, uint[] sids);

        public delegate void SourceStopvInternal(int ns, uint[] sids);

        public delegate void SourceRewindvInternal(int ns, uint[] sids);

        public delegate void SourcePausevInternal(int ns, uint[] sids);

        public delegate void SourcePlayInternal(uint sid);

        public delegate void SourceStopInternal(uint sid);

        public delegate void SourceRewindInternal(uint sid);

        public delegate void SourcePauseInternal(uint sid);

        public delegate void SourceQueueBuffersInternal(uint sid, int numEntries, uint[] bids);

        public delegate void SourceUnqueueBuffersInternal(uint sid, int numEntries, uint[] bids);

        private unsafe delegate void _GenBuffersInternal(int n, uint* buffers);

        public delegate void DeleteBuffersInternal(int n, uint[] buffers);

        public delegate bool IsBufferInternal(uint bid);

        public unsafe delegate void BufferDataInternal(uint bid, int format, void* data, int size, int freq);

        public delegate void BufferiInternal(uint bid, int param, int value);

        public delegate void GetBufferiInternal(uint bid, int param, out int value);

        public delegate void DopplerFactorInternal(float value);

        public delegate void DopplerVelocityInternal(float value);

        public delegate void SpeedOfSoundInternal(float value);

        public delegate void DistanceModelInternal(int distanceModel);

        #endregion

        #region Function Handles

        public static EnableInternal Enable;
        public static DisableInternal Disable;
        public static IsEnabledInternal IsEnabled;
        private static _GetStringInternal _GetString;
        public static GetBooleanvInternal GetBooleanv;
        public static GetIntegervInternal GetIntegerv;
        public static GetFloatvInternal GetFloatv;
        public static GetDoublevInternal GetDoublev;
        public static GetBooleanInternal GetBoolean;
        public static GetIntegerInternal GetInteger;
        public static GetFloatInternal GetFloat;
        public static GetDoubleInternal GetDouble;
        public static GetErrorInternal GetError;
        public static IsExtensionPresentInternal IsExtensionPresent;
        public static GetProcAddressInternal GetProcAddress;
        public static GetEnumValueInternal GetEnumValue;
        public static ListenerfInternal Listenerf;
        public static Listener3fInternal Listener3f;
        public static ListenerfvInternal Listenerfv;
        public static GetListenerfInternal GetListenerf;
        public static GetListener3fInternal GetListener3f;
        private static _GetListenerfvInternal _GetListenerfv;
        private static _GenSourcesInternal _GenSources;
        public static DeleteSourcesInternal DeleteSources;
        public static IsSourceInternal IsSource;
        public static SourcefInternal Sourcef;
        public static Source3fInternal Source3f;
        public static SourcefvInternal Sourcefv;
        public static SourceiInternal Sourcei;
        public static Source3iInternal Source3i;
        public static SourceivInternal Sourceiv;
        public static GetSourcefInternal GetSourcef;
        public static GetSource3fInternal GetSource3f;
        private static _GetSourcefvInternal _GetSourcefv;
        public static GetSourceiInternal GetSourcei;
        public static GetSource3iInternal GetSource3i;
        private static _GetSourceivInternal _GetSourceiv;
        public static SourcePlayvInternal SourcePlayv;
        public static SourceStopvInternal SourceStopv;
        public static SourceRewindvInternal SourceRewindv;
        public static SourcePausevInternal SourcePausev;
        public static SourcePlayInternal SourcePlay;
        public static SourceStopInternal SourceStop;
        public static SourceRewindInternal SourceRewind;
        public static SourcePauseInternal SourcePause;
        public static SourceQueueBuffersInternal SourceQueueBuffers;
        public static SourceUnqueueBuffersInternal SourceUnqueueBuffers;
        private static _GenBuffersInternal _GenBuffers;
        public static DeleteBuffersInternal DeleteBuffers;
        public static IsBufferInternal IsBuffer;
        public static BufferDataInternal BufferData;
        public static BufferiInternal Bufferi;
        public static GetBufferiInternal GetBufferi;
        public static DopplerFactorInternal DopplerFactor;
        public static DopplerVelocityInternal DopplerVelocity;
        public static SpeedOfSoundInternal SpeedOfSound;
        public static DistanceModelInternal DistanceModel;

        #endregion

        /// <summary>
        /// Initialize the library.
        /// </summary>
        /// <param name="lib">A pointer to the loaded library using the NativeLibrary class.</param>
        /// <returns>Whether initialization was successful.</returns>
        public static int Init(IntPtr lib)
        {
            GetFuncPointer(lib, "alEnable", ref Enable);
            GetFuncPointer(lib, "alDisable", ref Disable);
            GetFuncPointer(lib, "alIsEnabled", ref IsEnabled);
            GetFuncPointer(lib, "alGetString", ref _GetString);
            GetFuncPointer(lib, "alGetBooleanv", ref GetBooleanv);
            GetFuncPointer(lib, "alGetIntegerv", ref GetIntegerv);
            GetFuncPointer(lib, "alGetFloatv", ref GetFloatv);
            GetFuncPointer(lib, "alGetDoublev", ref GetDoublev);
            GetFuncPointer(lib, "alGetBoolean", ref GetBoolean);
            GetFuncPointer(lib, "alGetInteger", ref GetInteger);
            GetFuncPointer(lib, "alGetFloat", ref GetFloat);
            GetFuncPointer(lib, "alGetDouble", ref GetDouble);
            GetFuncPointer(lib, "alGetError", ref GetError);
            GetFuncPointer(lib, "alIsExtensionPresent", ref IsExtensionPresent);
            GetFuncPointer(lib, "alGetProcAddress", ref GetProcAddress);
            GetFuncPointer(lib, "alGetEnumValue", ref GetEnumValue);
            GetFuncPointer(lib, "alListenerf", ref Listenerf);
            GetFuncPointer(lib, "alListener3f", ref Listener3f);
            GetFuncPointer(lib, "alListenerfv", ref Listenerfv);
            GetFuncPointer(lib, "alGetListenerf", ref GetListenerf);
            GetFuncPointer(lib, "alGetListener3f", ref GetListener3f);
            GetFuncPointer(lib, "alGetListenerfv", ref _GetListenerfv);
            GetFuncPointer(lib, "alGenSources", ref _GenSources);
            GetFuncPointer(lib, "alDeleteSources", ref DeleteSources);
            GetFuncPointer(lib, "alIsSource", ref IsSource);
            GetFuncPointer(lib, "alSourcef", ref Sourcef);
            GetFuncPointer(lib, "alSource3f", ref Source3f);
            GetFuncPointer(lib, "alSourcefv", ref Sourcefv);
            GetFuncPointer(lib, "alSourcei", ref Sourcei);
            GetFuncPointer(lib, "alSource3i", ref Source3i);
            GetFuncPointer(lib, "alSourceiv", ref Sourceiv);
            GetFuncPointer(lib, "alGetSourcef", ref GetSourcef);
            GetFuncPointer(lib, "alGetSource3f", ref GetSource3f);
            GetFuncPointer(lib, "alGetSourcefv", ref _GetSourcefv);
            GetFuncPointer(lib, "alGetSourcei", ref GetSourcei);
            GetFuncPointer(lib, "alGetSource3i", ref GetSource3i);
            GetFuncPointer(lib, "alGetSourceiv", ref _GetSourceiv);
            GetFuncPointer(lib, "alSourcePlayv", ref SourcePlayv);
            GetFuncPointer(lib, "alSourceStopv", ref SourceStopv);
            GetFuncPointer(lib, "alSourceRewindv", ref SourceRewindv);
            GetFuncPointer(lib, "alSourcePausev", ref SourcePausev);
            GetFuncPointer(lib, "alSourcePlay", ref SourcePlay);
            GetFuncPointer(lib, "alSourceStop", ref SourceStop);
            GetFuncPointer(lib, "alSourceRewind", ref SourceRewind);
            GetFuncPointer(lib, "alSourcePause", ref SourcePause);
            GetFuncPointer(lib, "alSourceQueueBuffers", ref SourceQueueBuffers);
            GetFuncPointer(lib, "alSourceUnqueueBuffers", ref SourceUnqueueBuffers);
            GetFuncPointer(lib, "alGenBuffers", ref _GenBuffers);
            GetFuncPointer(lib, "alDeleteBuffers", ref DeleteBuffers);
            GetFuncPointer(lib, "alIsBuffer", ref IsBuffer);
            GetFuncPointer(lib, "alBufferData", ref BufferData);
            GetFuncPointer(lib, "alBufferi", ref Bufferi);
            GetFuncPointer(lib, "alGetBufferi", ref GetBufferi);
            GetFuncPointer(lib, "alDopplerFactor", ref DopplerFactor);
            GetFuncPointer(lib, "alDopplerVelocity", ref DopplerVelocity);
            GetFuncPointer(lib, "alSpeedOfSound", ref SpeedOfSound);
            GetFuncPointer(lib, "alDistanceModel", ref DistanceModel);

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

        public const int None = 0;
        public const int False = 0;
        public const int True = 1;
        public const int SourceRelative = 0x202;
        public const int ConeInnerAngle = 0x1001;
        public const int ConeOuterAngle = 0x1002;
        public const int Pitch = 0x1003;
        public const int Position = 0x1004;
        public const int Direction = 0x1005;
        public const int Velocity = 0x1006;
        public const int Looping = 0x1007;
        public const int Buffer = 0x1009;
        public const int Gain = 0x100A;
        public const int MinGain = 0x100D;
        public const int MaxGain = 0x100E;
        public const int Orientation = 0x100F;
        public const int SourceState = 0x1010;
        public const int Initial = 0x1011;
        public const int Playing = 0x1012;
        public const int Paused = 0x1013;
        public const int Stopped = 0x1014;
        public const int BuffersQueued = 0x1015;
        public const int BuffersProcessed = 0x1016;
        public const int SecOffset = 0x1024;
        public const int SampleOffset = 0x1025;
        public const int ByteOffset = 0x1026;
        public const int SourceType = 0x1027;
        public const int Static = 0x1028;
        public const int Streaming = 0x1029;
        public const int Undetermined = 0x1030;
        public const int FormatMono8 = 0x1100;
        public const int FormatMono16 = 0x1101;
        public const int FormatStereo8 = 0x1102;
        public const int FormatStereo16 = 0x1103;
        public const int ReferenceDistance = 0x1020;
        public const int RolloffFactor = 0x1021;
        public const int ConeOuterGain = 0x1022;
        public const int MaxDistance = 0x1023;
        public const int Frequency = 0x2001;
        public const int Bits = 0x2002;
        public const int Channels = 0x2003;
        public const int Size = 0x2004;
        public const int Unused = 0x2010;
        public const int Pending = 0x2011;
        public const int Processed = 0x2012;
        public const int NoError = False;
        public const int InvalidName = 0xA001;
        public const int InvalidEnum = 0xA002;
        public const int InvalidValue = 0xA003;
        public const int InvalidOperation = 0xA004;
        public const int OutOfMemory = 0xA005;
        public const int Vendor = 0xB001;
        public const int Version = 0xB002;
        public const int Renderer = 0xB003;
        public const int Extensions = 0xB004;
        public const int EnumDopplerFactor = 0xC000;
        public const int EnumDopplerVelocity = 0xC001;
        public const int EnumSpeedOfSound = 0xC003;
        public const int EnumDistanceModel = 0xD000;
        public const int InverseDistance = 0xD001;
        public const int InverseDistanceClamped = 0xD002;
        public const int LinearDistance = 0xD003;
        public const int LinearDistanceClamped = 0xD004;
        public const int ExponentDistance = 0xD005;
        public const int ExponentDistanceClamped = 0xD006;

        #endregion

        #region Functions

        public static string GetString(int param)
        {
            return Marshal.PtrToStringAnsi(_GetString(param));
        }

        public static void GetListenerfv(int param, out float[] values)
        {
            unsafe
            {
                int len;
                switch (param)
                {
                    case Gain:
                        len = 1;
                        break;
                    case Position:
                    case Velocity:
                        len = 3;
                        break;
                    case Orientation:
                        len = 6;
                        break;
                    default:
                        len = 0;
                        break;
                }

                values = new float[len];

                fixed (float* arrayPtr = values)
                {
                    _GetListenerfv(param, arrayPtr);
                }
            }
        }

        public static void GenSources(int n, out uint[] sources)
        {
            unsafe
            {
                sources = new uint[n];

                fixed (uint* arrayPtr = sources)
                {
                    _GenSources(n, arrayPtr);
                }
            }
        }

        public static void GenSource(out uint source)
        {
            GenSources(1, out uint[] sources);
            source = sources[0];
        }

        public static void DeleteSource(uint source)
        {
            DeleteSources(1, new[] {source});
        }

        public static void GetSourcefv(uint sid, int param, out float[] values)
        {
            unsafe
            {
                int len;
                switch (param)
                {
                    case Pitch:
                    case Gain:
                    case MaxDistance:
                    case RolloffFactor:
                    case ReferenceDistance:
                    case MinGain:
                    case MaxGain:
                    case ConeOuterGain:
                    case ConeInnerAngle:
                    case ConeOuterAngle:
                    case SecOffset:
                    case SampleOffset:
                    case ByteOffset:
                        len = 1;
                        break;
                    case Position:
                    case Velocity:
                    case Direction:
                        len = 3;
                        break;
                    default:
                        len = 0;
                        break;
                }

                values = new float[len];

                fixed (float* arrayPtr = values)
                {
                    _GetSourcefv(sid, param, arrayPtr);
                }
            }
        }

        public static void GetSourceiv(uint sid, int param, out int[] values)
        {
            unsafe
            {
                int len;
                switch (param)
                {
                    case MaxDistance:
                    case RolloffFactor:
                    case ReferenceDistance:
                    case ConeInnerAngle:
                    case ConeOuterAngle:
                    case SourceRelative:
                    case SourceType:
                    case Looping:
                    case Buffer:
                    case SourceState:
                    case BuffersQueued:
                    case BuffersProcessed:
                    case SecOffset:
                    case SampleOffset:
                    case ByteOffset:
                        len = 1;
                        break;
                    case Direction:
                        len = 3;
                        break;
                    default:
                        len = 0;
                        break;
                }

                values = new int[len];

                fixed (int* arrayPtr = values)
                {
                    _GetSourceiv(sid, param, arrayPtr);
                }
            }
        }

        public static void GenBuffers(int n, out uint[] buffers)
        {
            unsafe
            {
                buffers = new uint[n];

                fixed (uint* arrayPtr = buffers)
                {
                    _GenBuffers(n, arrayPtr);
                }
            }
        }

        public static void GenBuffer(out uint buffer)
        {
            GenBuffers(1, out uint[] buffers);
            buffer = buffers[0];
        }

        public static void DeleteBuffer(uint buffer)
        {
            DeleteBuffers(1, new[] {buffer});
        }

        #endregion
    }
}