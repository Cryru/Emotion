using Emotion.Audio;
using Emotion.Platform;
using Emotion.Standard.Logging;
using System.Runtime.InteropServices;
using Emotion.Testing;
using Emotion.Common;
using Emotion.Standard.Audio;

namespace Emotion.Droid;

public unsafe class AndroidAudioLayerBuffer
{
    public void* BufferPtr;
    public int SizeInFrames;
    public int FrameSizeInBytes;

    public int FramesContains;
    public int FramesRead;

    public int TotalByteSize { get => SizeInFrames * FrameSizeInBytes; }

    public bool CanRead = false;

    public AndroidAudioLayerBuffer(int bufferSizeFrames, int frameSizeInBytes)
    {
        SizeInFrames = bufferSizeFrames;
        FrameSizeInBytes = frameSizeInBytes;

        BufferPtr = NativeMemory.Alloc((nuint) (SizeInFrames * FrameSizeInBytes));
    }

    public Span<byte> GetSpan()
    {
        return new Span<byte>(BufferPtr, TotalByteSize);
    }
}

public sealed class AndroidAudioLayer : AudioLayer
{
    private AudioFormat _audioFormat;

    private const int BUFFER_COUNT = 2;

    private AndroidAudioLayerBuffer[] _uploadBuffers;
    private int _currentBuffer = 0;

    private IntPtr _stream;

    private AAudio.DataCallback _fillCallbackFunc;
    private AAudio.ErrorCallback _errorCallbackFunc;

    public AndroidAudioLayer(string name) : base(name)
    {
        AAudio.CreateStreamBuilder(out IntPtr streamBuilder);

        _fillCallbackFunc = FillData;
        AAudio.StreamBuilderSetCallback(streamBuilder, Marshal.GetFunctionPointerForDelegate(_fillCallbackFunc), 0);

        _errorCallbackFunc = ErrorCallback;
        AAudio.StreamBuilderSetErrorCallback(streamBuilder, Marshal.GetFunctionPointerForDelegate(_errorCallbackFunc), 0);

        AAudio.OpenStream(streamBuilder, out IntPtr stream);

        _stream = stream;

        int sampleRate = AAudio.GetStreamSampleRate(_stream);
        int channelCount = AAudio.GetStreamChannelCount(_stream);
        AAudio.StreamAudioFormat format = AAudio.GetStreamAudioFormat(_stream);

        int formatBits = 0;
        bool isFloat = false;
        switch (format)
        {
            case AAudio.StreamAudioFormat.AAUDIO_FORMAT_UNSPECIFIED:
            case AAudio.StreamAudioFormat.AAUDIO_FORMAT_INVALID:
            case AAudio.StreamAudioFormat.AAUDIO_FORMAT_IEC61937:
            case AAudio.StreamAudioFormat.AAUDIO_FORMAT_PCM_I24_PACKED:
                throw new Exception("Invalid format!");

            case AAudio.StreamAudioFormat.AAUDIO_FORMAT_PCM_I16:
                formatBits = 32;
                break;
            case AAudio.StreamAudioFormat.AAUDIO_FORMAT_PCM_FLOAT:
                formatBits = 32;
                isFloat = true;
                break;
            case AAudio.StreamAudioFormat.AAUDIO_FORMAT_PCM_I32:
                formatBits = 32;
                break;
        }

        _audioFormat = new AudioFormat(formatBits, isFloat, channelCount, sampleRate);
        Engine.Log.Trace($"Created stream with format: {_audioFormat}", "AndroidAudio");

        int bufferSize = _audioFormat.SecondsToFrames(AudioContext.BackendBufferExpectedAhead / 1000f);
        //bufferSize /= BUFFER_COUNT;

        int burstSize = AAudio.GetStreamFramesPerBurst(_stream) * 10;
        _uploadBuffers = new AndroidAudioLayerBuffer[BUFFER_COUNT];
        for (int i = 0; i < _uploadBuffers.Length; i++)
        {
            _uploadBuffers[i] = new AndroidAudioLayerBuffer(bufferSize, _audioFormat.FrameSize);
        }
    }

    private void ErrorCallback(IntPtr stream, IntPtr userData, int errorCode)
    {
        Engine.Log.Warning($"Audio layer {Name} encountered error {errorCode}", MessageSource.Audio);
    }

    public unsafe int FillData(IntPtr stream, IntPtr userData, IntPtr dstBufferPtr, int framesGet)
    {
        int bytes = framesGet * _audioFormat.FrameSize;
        Span<byte> dstBuffer = new Span<byte>((void*)dstBufferPtr, bytes);
        dstBuffer.Fill(0);

        while (framesGet > 0)
        {
            AndroidAudioLayerBuffer buffer = _uploadBuffers[_currentBuffer];
            if (!buffer.CanRead)
            {
                _currentBuffer++;
                _currentBuffer %= BUFFER_COUNT;
                buffer = _uploadBuffers[_currentBuffer];
                if (!buffer.CanRead) return 0;
            }

            Span<byte> currentSrcSpan = buffer.GetSpan();
            currentSrcSpan = currentSrcSpan.Slice(buffer.FramesRead * buffer.FrameSizeInBytes);

            int framesInBuffer = buffer.FramesContains;
            int framesFromThisBuffer = Math.Min(framesInBuffer, framesGet);

            currentSrcSpan = currentSrcSpan.Slice(0, framesFromThisBuffer * buffer.FrameSizeInBytes);
            currentSrcSpan.CopyTo(dstBuffer);
            dstBuffer = dstBuffer.Slice(framesFromThisBuffer * buffer.FrameSizeInBytes);

            // Update buffer trackers.
            buffer.FramesContains -= framesFromThisBuffer;
            buffer.FramesRead += framesFromThisBuffer;
            if (buffer.FramesContains == 0)
            {
                buffer.CanRead = false;
                _currentBuffer++;
                _currentBuffer %= BUFFER_COUNT;
            }

            framesGet -= framesFromThisBuffer;
        }

        // 0 Continue
        // 1 Stop
        return 0;
    }

    public override void Dispose()
    {
        base.Dispose();
        AAudio.StreamRequestStop(_stream);
    }

    protected override unsafe void UpdateBackend()
    {
        var state = AAudio.StreamGetState(_stream);
        if (Status == PlaybackStatus.Playing)
        {
            // Emotion is playing but Android isn't.
            if (state != AAudio.StreamState.AAUDIO_STREAM_STATE_STARTING && state != AAudio.StreamState.AAUDIO_STREAM_STATE_STARTED)
            {
                // Wait for buffers to top up!
                bool allBuffersReady = true;
                for (int i = 0; i < _uploadBuffers.Length; i++)
                {
                    AndroidAudioLayerBuffer buffer = _uploadBuffers[i];
                    if (!buffer.CanRead)
                        allBuffersReady = false;
                }

                if (allBuffersReady)
                {
                    int resp = AAudio.StreamRequestStart(_stream);
                    Engine.Log.Trace($"Stream {Name} start: {resp}", "AndroidAudio");
                }
                else
                {
                    Engine.Log.Trace($"Stream {Name} wants to start, but waiting for buffers", "AndroidAudio");
                }
            }
        }
        else
        {
            // Emotion is not playing, but Android is.
            if (state == AAudio.StreamState.AAUDIO_STREAM_STATE_STARTED)
            {
                // Wait for buffers to play!
                bool allBuffersReady = true;
                for (int i = 0; i < _uploadBuffers.Length; i++)
                {
                    AndroidAudioLayerBuffer buffer = _uploadBuffers[i];
                    if (buffer.CanRead)
                        allBuffersReady = false;
                }

                if (allBuffersReady)
                {
                    int resp = AAudio.StreamRequestStop(_stream);
                    Engine.Log.Trace($"Stream {Name} stop: {resp}", "AndroidAudio");
                }
                else
                {
                    Engine.Log.Trace($"Stream {Name} wants to stop, but waiting for buffers", "AndroidAudio");
                }
            }
        }

        if (Status == PlaybackStatus.Playing)
        {
            // Fill empty buffers (but not more than one!)
            for (int i = 0; i < _uploadBuffers.Length; i++)
            {
                AndroidAudioLayerBuffer buffer = _uploadBuffers[i];
                Assert.True(buffer.FramesContains >= 0);
                if (!buffer.CanRead)
                {
                    int framesGotten = BackendGetData(_audioFormat, buffer.SizeInFrames, buffer.GetSpan());
                    buffer.FramesContains = framesGotten;
                    buffer.FramesRead = 0;
                    buffer.CanRead = true;
                    break;
                }
            }
        }
    }
}

public class AAudio
{
    public const string LibName = "aaudio";

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AAudio_createStreamBuilder")]
    public static extern int CreateStreamBuilder(out IntPtr streamBuilder);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AAudioStreamBuilder_setDataCallback")]
    public static extern int StreamBuilderSetCallback(IntPtr streamBuilder, IntPtr callbackFunc, IntPtr userData);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AAudioStreamBuilder_setErrorCallback")]
    public static extern int StreamBuilderSetErrorCallback(IntPtr streamBuilder, IntPtr callbackFunc, IntPtr userData);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AAudioStreamBuilder_setPerformanceMode")]
    public static extern int StreamBuilderSetPerformanceMode(IntPtr streamBuilder, StreamBuilderPerformanceMode mode);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AAudioStreamBuilder_openStream")]
    public static extern int OpenStream(IntPtr builder, out IntPtr audioStream);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AAudioStream_getBufferCapacityInFrames")]
    public static extern int GetStreamBufferCapacityInFrames(IntPtr audioStream);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AAudioStream_getFramesPerBurst")]
    public static extern int GetStreamFramesPerBurst(IntPtr audioStream);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AAudioStream_getFormat")]
    public static extern StreamAudioFormat GetStreamAudioFormat(IntPtr audioStream);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AAudioStream_getChannelCount")]
    public static extern int GetStreamChannelCount(IntPtr audioStream);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AAudioStream_getSampleRate")]
    public static extern int GetStreamSampleRate(IntPtr audioStream);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AAudioStream_getState")]
    public static extern StreamState StreamGetState(IntPtr audioStream);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AAudioStream_requestStart")]
    public static extern int StreamRequestStart(IntPtr audioStream);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AAudioStream_requestStop")]
    public static extern int StreamRequestStop(IntPtr audioStream);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AAudioStream_requestPause")]
    public static extern int StreamRequestPause(IntPtr audioStream);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AAudioStream_requestFlush")]
    public static extern int StreamRequestFlush(IntPtr audioStream);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AAudioStream_write")]
    public static extern int StreamWrite(IntPtr audioStream, IntPtr buffer, int frames, ulong timeoutNanos);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AAudioStream_getXRunCount")]
    public static extern int StreamGetXRun(IntPtr stream);

    public enum StreamState : int
    {
        AAUDIO_STREAM_STATE_UNINITIALIZED = 0,
        AAUDIO_STREAM_STATE_UNKNOWN,
        AAUDIO_STREAM_STATE_OPEN,
        AAUDIO_STREAM_STATE_STARTING,
        AAUDIO_STREAM_STATE_STARTED,
        AAUDIO_STREAM_STATE_PAUSING,
        AAUDIO_STREAM_STATE_PAUSED,
        AAUDIO_STREAM_STATE_FLUSHING,
        AAUDIO_STREAM_STATE_FLUSHED,
        AAUDIO_STREAM_STATE_STOPPING,
        AAUDIO_STREAM_STATE_STOPPED,
        AAUDIO_STREAM_STATE_CLOSING,
        AAUDIO_STREAM_STATE_CLOSED,
        AAUDIO_STREAM_STATE_DISCONNECTED
    }

    public enum StreamAudioFormat : int
    {
        AAUDIO_FORMAT_INVALID = -1,
        AAUDIO_FORMAT_UNSPECIFIED = 0,
        AAUDIO_FORMAT_PCM_I16,
        AAUDIO_FORMAT_PCM_FLOAT,
        AAUDIO_FORMAT_PCM_I24_PACKED,
        AAUDIO_FORMAT_PCM_I32,
        AAUDIO_FORMAT_IEC61937
    };

    public enum StreamBuilderPerformanceMode
    {
        AAUDIO_PERFORMANCE_MODE_NONE = 10,
        AAUDIO_PERFORMANCE_MODE_POWER_SAVING,
        AAUDIO_PERFORMANCE_MODE_LOW_LATENCY
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DataCallback(IntPtr stream, IntPtr userData, IntPtr dstBuffer, int framesGet);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ErrorCallback(IntPtr stream, IntPtr userData, int error);
}

public class AndroidAudio : AudioContext
{
    public AndroidAudio(PlatformBase platform) : base(platform)
    {
    }

    public override AudioLayer CreatePlatformAudioLayer(string layerName)
    {
        return new AndroidAudioLayer(layerName);
    }
}