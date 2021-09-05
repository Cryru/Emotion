#region Using

using System;
using System.Runtime.InteropServices;

#endregion

// ReSharper disable UnusedMember.Global

// ReSharper disable once CheckNamespace
namespace OpenAL
{
    public static class Al
    {
        public const string OPEN_AL_DLL = "OpenAL";

        #region Enum

        public const int NONE = 0;
        public const int FALSE = 0;
        public const int TRUE = 1;
        public const int SOURCE_RELATIVE = 0x202;
        public const int CONE_INNER_ANGLE = 0x1001;
        public const int CONE_OUTER_ANGLE = 0x1002;
        public const int PITCH = 0x1003;
        public const int POSITION = 0x1004;
        public const int DIRECTION = 0x1005;
        public const int VELOCITY = 0x1006;
        public const int LOOPING = 0x1007;
        public const int BUFFER = 0x1009;
        public const int GAIN = 0x100A;
        public const int MIN_GAIN = 0x100D;
        public const int MAX_GAIN = 0x100E;
        public const int ORIENTATION = 0x100F;
        public const int SOURCE_STATE = 0x1010;
        public const int INITIAL = 0x1011;
        public const int PLAYING = 0x1012;
        public const int PAUSED = 0x1013;
        public const int STOPPED = 0x1014;
        public const int BUFFERS_QUEUED = 0x1015;
        public const int BUFFERS_PROCESSED = 0x1016;
        public const int SEC_OFFSET = 0x1024;
        public const int SAMPLE_OFFSET = 0x1025;
        public const int BYTE_OFFSET = 0x1026;
        public const int SOURCE_TYPE = 0x1027;
        public const int STATIC = 0x1028;
        public const int STREAMING = 0x1029;
        public const int UNDETERMINED = 0x1030;
        public const int FORMAT_MONO8 = 0x1100;
        public const int FORMAT_MONO16 = 0x1101;
        public const int FORMAT_STEREO8 = 0x1102;
        public const int FORMAT_STEREO16 = 0x1103;
        public const int FORMAT_STEREO32F = 0x10011;
        public const int REFERENCE_DISTANCE = 0x1020;
        public const int ROLLOFF_FACTOR = 0x1021;
        public const int CONE_OUTER_GAIN = 0x1022;
        public const int MAX_DISTANCE = 0x1023;
        public const int FREQUENCY = 0x2001;
        public const int BITS = 0x2002;
        public const int CHANNELS = 0x2003;
        public const int SIZE = 0x2004;
        public const int UNUSED = 0x2010;
        public const int PENDING = 0x2011;
        public const int PROCESSED = 0x2012;
        public const int NO_ERROR = FALSE;
        public const int INVALID_NAME = 0xA001;
        public const int INVALID_ENUM = 0xA002;
        public const int INVALID_VALUE = 0xA003;
        public const int INVALID_OPERATION = 0xA004;
        public const int OUT_OF_MEMORY = 0xA005;
        public const int VENDOR = 0xB001;
        public const int VERSION = 0xB002;
        public const int RENDERER = 0xB003;
        public const int EXTENSIONS = 0xB004;
        public const int ENUM_DOPPLER_FACTOR = 0xC000;
        public const int ENUM_DOPPLER_VELOCITY = 0xC001;
        public const int ENUM_SPEED_OF_SOUND = 0xC003;
        public const int ENUM_DISTANCE_MODEL = 0xD000;
        public const int INVERSE_DISTANCE = 0xD001;
        public const int INVERSE_DISTANCE_CLAMPED = 0xD002;
        public const int LINEAR_DISTANCE = 0xD003;
        public const int LINEAR_DISTANCE_CLAMPED = 0xD004;
        public const int EXPONENT_DISTANCE = 0xD005;
        public const int EXPONENT_DISTANCE_CLAMPED = 0xD006;

        #endregion

        #region Functions

        [DllImport(OPEN_AL_DLL, EntryPoint = "alEnable")]
        public static extern void Enable(int capability);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alDisable")]
        public static extern void Disable(int capability);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alIsEnabled")]
        public static extern bool IsEnabled(int capability);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetString")]
        private static extern IntPtr _GetString(int param);

        public static string GetString(int param)
        {
            return Marshal.PtrToStringAnsi(_GetString(param));
        }

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetBooleanv")]
        public static extern void GetBooleanv(int param, out bool data);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetIntegerv")]
        public static extern void GetIntegerv(int param, out int data);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetFloatv")]
        public static extern void GetFloatv(int param, out float data);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetDoublev")]
        public static extern void GetDoublev(int param, out double data);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetBoolean")]
        public static extern bool GetBoolean(int param);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetInteger")]
        public static extern int GetInteger(int param);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetFloat")]
        public static extern float GetFloat(int param);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetDouble")]
        public static extern double GetDouble(int param);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetError")]
        public static extern int GetError();

        [DllImport(OPEN_AL_DLL, EntryPoint = "alIsExtensionPresent")]
        public static extern bool IsExtensionPresent(string extname);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetProcAddress")]
        public static extern unsafe void* GetProcAddress(string fname);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetEnumValue")]
        public static extern int GetEnumValue(string ename);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alListenerf")]
        public static extern void Listenerf(int param, float value);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alListener3f")]
        public static extern void Listener3f(int param, float value1, float value2, float value3);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alListenerfv")]
        public static extern void Listenerfv(int param, float[] values);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetListenerf")]
        public static extern void GetListenerf(int param, out float value);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetListener3f")]
        public static extern void GetListener3f(int param, out float value1, out float value2, out float value3);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetListenerfv")]
        private static extern unsafe void _GetListenerfv(int param, float* values);

        public static void GetListenerfv(int param, out float[] values)
        {
            unsafe
            {
                int len;
                switch (param)
                {
                    case GAIN:
                        len = 1;
                        break;
                    case POSITION:
                    case VELOCITY:
                        len = 3;
                        break;
                    case ORIENTATION:
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

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGenSources")]
        private static extern unsafe void _GenSources(int n, uint* sources);

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

        [DllImport(OPEN_AL_DLL, EntryPoint = "alDeleteSources")]
        public static extern void DeleteSources(int n, uint[] sources);

        public static void DeleteSource(uint source)
        {
            DeleteSources(1, new[] {source});
        }

        [DllImport(OPEN_AL_DLL, EntryPoint = "alIsSource")]
        public static extern bool IsSource(uint sid);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alSourcef")]
        public static extern void Sourcef(uint sid, int param, float value);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alSource3f")]
        public static extern void Source3f(uint sid, int param, float value1, float value2, float value3);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alSourcefv")]
        public static extern void Sourcefv(uint sid, int param, float[] values);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alSourcei")]
        public static extern void Sourcei(uint sid, int param, int value);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alSource3i")]
        public static extern void Source3i(uint sid, int param, int value1, int value2, int value3);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alSourceiv")]
        public static extern void Sourceiv(uint sid, int param, int[] values);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetSourcef")]
        public static extern void GetSourcef(uint sid, int param, out float value);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetSource3f")]
        public static extern void GetSource3f(uint sid, int param, out float value1, out float value2, out float value3);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetSourcefv")]
        private static extern unsafe void _GetSourcefv(uint sid, int param, float* values);

        public static void GetSourcefv(uint sid, int param, out float[] values)
        {
            unsafe
            {
                int len;
                switch (param)
                {
                    case PITCH:
                    case GAIN:
                    case MAX_DISTANCE:
                    case ROLLOFF_FACTOR:
                    case REFERENCE_DISTANCE:
                    case MIN_GAIN:
                    case MAX_GAIN:
                    case CONE_OUTER_GAIN:
                    case CONE_INNER_ANGLE:
                    case CONE_OUTER_ANGLE:
                    case SEC_OFFSET:
                    case SAMPLE_OFFSET:
                    case BYTE_OFFSET:
                        len = 1;
                        break;
                    case POSITION:
                    case VELOCITY:
                    case DIRECTION:
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

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetSourcei")]
        public static extern void GetSourcei(uint sid, int param, out int value);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetSource3i")]
        public static extern void GetSource3i(uint sid, int param, out int value1, out int value2, out int value3);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetSourceiv")]
        private static extern unsafe void _GetSourceiv(uint sid, int param, int* values);

        public static void GetSourceiv(uint sid, int param, out int[] values)
        {
            unsafe
            {
                int len;
                switch (param)
                {
                    case MAX_DISTANCE:
                    case ROLLOFF_FACTOR:
                    case REFERENCE_DISTANCE:
                    case CONE_INNER_ANGLE:
                    case CONE_OUTER_ANGLE:
                    case SOURCE_RELATIVE:
                    case SOURCE_TYPE:
                    case LOOPING:
                    case BUFFER:
                    case SOURCE_STATE:
                    case BUFFERS_QUEUED:
                    case BUFFERS_PROCESSED:
                    case SEC_OFFSET:
                    case SAMPLE_OFFSET:
                    case BYTE_OFFSET:
                        len = 1;
                        break;
                    case DIRECTION:
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

        [DllImport(OPEN_AL_DLL, EntryPoint = "alSourcePlayv")]
        public static extern void SourcePlayv(int ns, uint[] sids);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alSourceStopv")]
        public static extern void SourceStopv(int ns, uint[] sids);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alSourceRewindv")]
        public static extern void SourceRewindv(int ns, uint[] sids);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alSourcePausev")]
        public static extern void SourcePausev(int ns, uint[] sids);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alSourcePlay")]
        public static extern void SourcePlay(uint sid);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alSourceStop")]
        public static extern void SourceStop(uint sid);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alSourceRewind")]
        public static extern void SourceRewind(uint sid);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alSourcePause")]
        public static extern void SourcePause(uint sid);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alSourceQueueBuffers")]
        public static extern unsafe void SourceQueueBuffers(uint sid, int numEntries, uint* bids);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alSourceUnqueueBuffers")]
        public static extern void SourceUnqueueBuffers(uint sid, int numEntries, uint[] bids);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGenBuffers")]
        private static extern unsafe void _GenBuffers(int n, uint* buffers);

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

        [DllImport(OPEN_AL_DLL, EntryPoint = "alDeleteBuffers")]
        public static extern void DeleteBuffers(int n, uint[] buffers);

        public static void DeleteBuffer(uint buffer)
        {
            DeleteBuffers(1, new[] {buffer});
        }

        [DllImport(OPEN_AL_DLL, EntryPoint = "alIsBuffer")]
        public static extern bool IsBuffer(uint bid);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alBufferData")]
        public static extern void BufferData(uint bid, int format, byte[] data, int size, int freq);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alBufferi")]
        public static extern void Bufferi(uint bid, int param, int value);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alGetBufferi")]
        public static extern void GetBufferi(uint bid, int param, out int value);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alDopplerFactor")]
        public static extern void DopplerFactor(float value);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alDopplerVelocity")]
        public static extern void DopplerVelocity(float value);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alSpeedOfSound")]
        public static extern void SpeedOfSound(float value);

        [DllImport(OPEN_AL_DLL, EntryPoint = "alDistanceModel")]
        public static extern void DistanceModel(int distanceModel);

        #endregion
    }
}