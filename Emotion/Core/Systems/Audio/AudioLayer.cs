#nullable enable

#region Using

using System.Linq;
using Emotion.Core.Systems.Audio;
using Emotion.Core.Systems.IO;
using Emotion.Core.Systems.Logging;
using Emotion.Standard.DataStructures;

#endregion

namespace Emotion.Core.Systems.Audio;

/// <summary>
/// Object for internal reference by the audio implementation of the platform.
/// </summary>
public abstract partial class AudioLayer
{
    /// <summary>
    /// The layer's friendly name.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// The layer's volume modifier is applied to the current volume
    /// which in turn is controlled by things such as fades.
    /// [0-1]
    /// </summary>
    public float VolumeModifier { get; set; } = 1;

    /// <summary>
    /// Get the volume applied by SetVolume. This will update over time.
    /// </summary>
    public float AppliedVolume
    {
        get
        {
            if (_userModifier == null) return 1f;
            return _userModifier.GetVolumeAt(_playHead);
        }
    }

    /// <summary>
    /// Whether the layer is destroyed.
    /// </summary>
    public bool Disposed { get; protected set; }

    /// <summary>
    /// The status of the audio layer. This should be async from the actual state.
    /// </summary>
    public PlaybackStatus Status
    {
        get => _status;
        protected set
        {
            if (value != _status)
            {
                OnStatusChanged?.Invoke(value, _status);
                _status = value;
            }
        }
    }

    private PlaybackStatus _status = PlaybackStatus.NotPlaying;

    /// <summary>
    /// Whether the current track is being looped.
    /// </summary>
    public bool LoopingCurrent
    {
        get => _looping;
        set
        {
            if (_looping == value) return;
            _looping = value;
            InvalidateCurrentTrack();
        }
    }

    private bool _looping;

    /// <summary>
    /// The track currently playing - if any.
    /// </summary>
    public AudioTrack? CurrentTrack
    {
        get
        {
            if (_updateCurrentTrack)
                lock (_playlist)
                {
                    return _playlist.Count == 0 ? null : _playlist[0];
                }

            return _currentTrack;
        }
    }

    /// <summary>
    /// What percentage (0-1) of the track has finished playing.
    /// </summary>
    public float Progress
    {
        get
        {
            if (_playHead == 0) return 0f;

            if (_updateCurrentTrack)
                lock (_playlist)
                {
                    AudioTrack? willBeCurrent = _playlist.Count == 0 ? null : _playlist[0];
                    if (willBeCurrent == _currentTrack)
                        return (float) _playHead / _totalSamplesConv;
                    return 0;
                }

            return (float) _playHead / _totalSamplesConv;
        }
    }

    /// <summary>
    /// How far along the duration of the file the track has finished playing.
    /// </summary>
    public float Playback
    {
        get
        {
            AudioTrack? currentTrack = _currentTrack;
            if (currentTrack == null) return 0;
            return Progress * currentTrack.File.Duration;
        }
    }

    /// <summary>
    /// The current playlist.
    /// Do not use this except for debugging and such.
    /// To get the current track use "CurrentTrack"
    /// </summary>
    public AudioAsset[] Playlist
    {
        get
        {
            lock (_playlist)
            {
                return _playlist.Select(x => x.File).ToArray();
            }
        }
    }

    /// <summary>
    /// Called when the current track loops.
    /// The input parameter is the track which looped.
    /// </summary>
    public event Action<AudioAsset>? OnTrackLoop;

    /// <summary>
    /// Called when the current track changes.
    /// The first parameter is the old track, the second is the new one.
    /// If there is no further track the new track parameter will be null.
    /// </summary>
    public event Action<AudioAsset?, AudioAsset?>? OnTrackChanged;

    /// <summary>
    /// Called when the layer status changes.
    /// </summary>
    public event Action<PlaybackStatus, PlaybackStatus>? OnStatusChanged;

    /// <summary>
    /// The audio format audio is streamed in. This is set by the backend.
    /// </summary>
    public AudioFormat CurrentStreamingFormat { get; private set; }

    // For fade effects volume is calculated per chunk of frames, rather than for every frame.
    private const int VOLUME_MODULATION_FRAME_GRANULARITY = 100; //tudu delete
    private const int INITIAL_INTERNAL_BUFFER_SIZE = 4000; // 4 * sizeof(float) = 16kb

    private List<AudioTrack> _playlist = new(); // Always read and written in locks

    private float[] _internalBuffer; // Internal memory to keep resamples frames for applying post proc to.
    private float[] _internalBufferCrossFade; // Same memory, but for the second track when crossfading.

    private int _playHead; // The progress into the current track, relative to the converted format.
    private int _totalSamplesConv; // The total sample count in the dst format. Used to know where _playHead is relative to the total.

    // Track state
    private bool _updateCurrentTrack = true;
    private AudioTrack? _currentTrack;
    private AudioTrack? _nextTrack;
    private int _loopCount; // Number of times the current track has looped. 0 if it hasn't.

    protected AudioLayer(string name)
    {
        Name = name;
        _internalBuffer = new float[INITIAL_INTERNAL_BUFFER_SIZE];
        _internalBufferCrossFade = new float[INITIAL_INTERNAL_BUFFER_SIZE];

        CurrentStreamingFormat = new AudioFormat();
    }

    #region API

    /// <summary>
    /// Sets the track to be played next in the playlist. If the playlist is empty and the layer isn't paused the track is
    /// played immediately.
    /// </summary>
    /// <param name="track">The track to play next.</param>
    public void PlayNext(AudioTrack track)
    {
        if (track.File == null) return;

        lock (_playlist)
        {
            if (_playlist.Count == 0)
                _playlist.Add(track);
            else
                _playlist.Insert(1, track);
        }

        InvalidateCurrentTrack();
        if (Status == PlaybackStatus.NotPlaying) Status = PlaybackStatus.Playing;
    }

    /// <inheritdoc cref="PlayNext(AudioTrack)" />
    public void PlayNext(AudioAsset file)
    {
        PlayNext(new AudioTrack(file));
    }

    /// <summary>
    /// Causes a crossfade transition from the current track into the next.
    /// If the current track doesn't have cross fade enabled you must supply a duration here.
    /// </summary>
    public void FadeCurrentTrackIntoNext(float crossFadeDurationSeconds)
    {
        _triggerEffectDuration = crossFadeDurationSeconds;
        _triggerCrossFadeToNext = true;
    }

    /// <summary>
    /// Adds the track to the back of the playlist. If the playlist is empty and the layer isn't paused the track is played
    /// immediately.
    /// </summary>
    /// <param name="track">The track to play.</param>
    public void AddToQueue(AudioTrack track)
    {
        if (track.File == null) return;

        lock (_playlist)
        {
            _playlist.Add(track);
        }

        InvalidateCurrentTrack();
        if (Status == PlaybackStatus.NotPlaying) Status = PlaybackStatus.Playing;
    }

    /// <inheritdoc cref="AddToQueue(AudioTrack)" />
    public void AddToQueue(AudioAsset file)
    {
        AddToQueue(new AudioTrack(file));
    }

    /// <summary>
    /// Stop all previous playback, clear the playlist, and play the provided track.
    /// This is essentially the same as calling Stop and then PlayNext but causes less state transitions and doesn't involve
    /// the platform.
    /// </summary>
    public void QuickPlay(AudioTrack track)
    {
        if (track.File == null) return;

        lock (_playlist)
        {
            _playlist.Clear();
            _playlist.Add(track);
        }

        InvalidateCurrentTrack();
        if (Status == PlaybackStatus.NotPlaying) Status = PlaybackStatus.Playing;
    }

    /// <inheritdoc cref="QuickPlay(AudioAsset)" />
    public void QuickPlay(AudioAsset file)
    {
        QuickPlay(new AudioTrack(file));
    }

    /// <summary>
    /// Resume playback, if paused. If currently playing nothing happens. If currently not playing - start playing.
    /// </summary>
    public void Resume()
    {
        Status = CurrentTrack != null ? PlaybackStatus.Playing : PlaybackStatus.NotPlaying;
    }

    /// <summary>
    /// Pause playback. If currently not playing anything the layer is paused anyway, and will need to be resumed.
    /// </summary>
    public void Pause()
    {
        Status = PlaybackStatus.Paused;
        InvalidateCurrentTrack();
    }

    /// <summary>
    /// Stop all playback, and clear the playlist.
    /// </summary>
    public void Stop()
    {
        lock (_playlist)
        {
            _playlist.Clear();
        }

        if (Status == PlaybackStatus.Playing) Status = PlaybackStatus.NotPlaying;
        InvalidateCurrentTrack();
    }

    /// <summary>
    /// Stop playing all tracks, after fading out the current one.
    /// </summary>
    public bool StopWithFade(float fadeDurationSeconds)
    {
        if (_currentTrack == null) return false;

        _triggeredFadeOutStopPlaylist ??= new List<AudioTrack>();
        _triggeredFadeOutStopPlaylist.Clear();
        lock (_playlist)
        {
            _triggeredFadeOutStopPlaylist.AddRange(_playlist);
        }

        _triggerEffectDuration = fadeDurationSeconds;
        _triggerFadeOutAndStop = true;
        return true;
    }

    /// <summary>
    /// Set the volume for the layer over a period, or instant (if 0)
    /// This doesn't change the layer's volume modifier, or global volume, but is multiplicative to it.
    /// </summary>
    /// <param name="volumeGoal"></param>
    /// <param name="ms"></param>
    public void SetVolume(float volumeGoal, int ms)
    {
        float startVol = AppliedVolume;
        int volumeChangeSamples = CurrentStreamingFormat.SecondsToFrames(ms / 1000f) * CurrentStreamingFormat.Channels;
        _userModifier = new VolumeModulationEffect(startVol, volumeGoal, _playHead, _playHead + volumeChangeSamples, EffectPosition.Absolute);
    }

    #endregion

    #region Stream Logic

    /// <summary>
    /// The max number of blocks to sample ahead.
    /// Each block is between AudioUpdateRate and AudioUpdateRate * 2 ms of audio data
    /// But if too small, then blocks will be overriden before you hear them.
    /// Unless the driver is lagging significantly behind you should never have more than 1-2 blocks ready.
    /// </summary>
    public static int MaxDataBlocks = 10;

#if DEBUG
    /// <summary>
    /// Total bytes allocated for all data blocks. Blocks and memory are shared between audio layers.
    /// </summary>
    public static int MetricAllocatedDataBlocks;

    /// <summary>
    /// How many ms of data is currently stored in all ready blocks.
    /// </summary>
    public int MetricDataStoredInBlocks;

    /// <summary>
    /// How many frames were dropped because the backend didn't request them.
    /// </summary>
    public int MetricBackendMissedFrames;

    /// <summary>
    /// How many frames was the buffer starved of - AKA didn't get them from ready blocks
    /// but had to resample on the fly.
    /// </summary>
    public int MetricStarved;

#endif

    private static ObjectPool<AudioDataBlock> _dataPool = new ObjectPool<AudioDataBlock>(null, MaxDataBlocks);
    private Queue<AudioDataBlock> _readyBlocks = new Queue<AudioDataBlock>();

    // Debug
    private Stopwatch? _updateTimer;
    private UpdateScopeTracker _inUpdateLoop = new ();

    private class UpdateScopeTracker : IDisposable
    {
        public bool Disposed;

        public void Reset()
        {
            Disposed = false;
        }

        public void Dispose()
        {
            Disposed = true;
        }
    }

    private struct UpdateScopeTrackerUsingStatementHack : IDisposable // Hack to remove allocations from update tracking
    {
        public UpdateScopeTracker Tracker;

        public UpdateScopeTrackerUsingStatementHack(UpdateScopeTracker tracker)
        {
            Tracker = tracker;
            tracker.Reset();
        }

        public void Dispose()
        {
            Tracker.Dispose();
        }
    }

    private class AudioDataBlock
    {
        public byte[] Data = Array.Empty<byte>();
        public int FramesWritten;
        public int FramesRead;
    }

    /// <summary>
    /// Process audio data ahead.
    /// </summary>
    /// <param name="timePassed">Time to process, in milliseconds</param>
    /// <param name="exact">Whether to update exactly that amount. Used for testing.</param>
    public void Update(int timePassed, bool exact = false)
    {
        using var _ = new UpdateScopeTrackerUsingStatementHack(_inUpdateLoop);

        UpdateCurrentTrack();

        // Update the backend. This will cause it to call BackendGetData
        UpdateBackend();

        if (Status != PlaybackStatus.Playing || _currentTrack == null) return;

#if DEBUG
        _updateTimer ??= Stopwatch.StartNew();

        if (_updateTimer.ElapsedMilliseconds > 5000)
        {
            MetricStarved = 0;

            MetricDataStoredInBlocks = 0;
            foreach (AudioDataBlock block in _readyBlocks)
            {
                MetricDataStoredInBlocks += block.FramesWritten - block.FramesRead;
            }

            MetricDataStoredInBlocks = (int) (CurrentStreamingFormat.GetSoundDuration(MetricDataStoredInBlocks * CurrentStreamingFormat.FrameSize) * 1000);
            _updateTimer.Restart();
        }
#endif

        // Prevent spiral of death, hopefully.
        const int tooMuchTime = 50;
        if (timePassed > tooMuchTime && !exact)
        {
            int extraTime = timePassed - tooMuchTime;

            timePassed = tooMuchTime;

#if DEBUG
            MetricStarved += extraTime * CurrentStreamingFormat.SampleSize / 1000;
#endif
        }

        // Convert time to frames and bytes.
        int framesToGet = CurrentStreamingFormat.SecondsToFrames(timePassed / 1000f);

        // If none blocks left or one block with little data in it, then do a larger request to prevent starvation.
        if (!exact)
            if (!_readyBlocks.TryPeek(out AudioDataBlock? topBlock) || (_readyBlocks.Count == 1 && topBlock.FramesWritten - topBlock.FramesRead < framesToGet))
                framesToGet *= 2;

        // Buffer data in advance, so the next update can be as fast as possible (just a copy).
        BufferDataInAdvance(framesToGet);
    }

    private void InvalidateCurrentTrack()
    {
        _updateCurrentTrack = true;
    }

    private void UpdateCurrentTrack()
    {
        if (!_updateCurrentTrack) return;
        _updateCurrentTrack = false;

        // Should only be called in the update loop, such as by GetDataToBuffer and Update
        Assert(_inUpdateLoop != null && !_inUpdateLoop.Disposed);

        // Get the currently playing track.
        AudioTrack? currentTrack;
        AudioTrack? nextTrack;
        lock (_playlist)
        {
            if (_playlist.Count == 0)
            {
                currentTrack = null;
                nextTrack = null;
                InvalidateAudioBlocks();
                if (Status == PlaybackStatus.Playing) Status = PlaybackStatus.NotPlaying;
            }
            else
            {
                currentTrack = _playlist[0];

                if (LoopingCurrent)
                    nextTrack = currentTrack;
                else if (_playlist.Count > 1)
                    nextTrack = _playlist[1];
                else
                    nextTrack = null;
            }
        }

        bool currentChanged = _currentTrack != currentTrack;
        bool nextChanged = _nextTrack != nextTrack;

        int prevTrackPlayHead = _playHead;

        // If both the current and next changed (or just current if no next)
        // that means everything has changed. This usually happens when the current track
        // is over and we transition into the next one. It could also be the result of a stop + new playlist.
        if (currentChanged && (nextChanged || nextTrack == null)) _playHead = 0;

        // If we have a new track.
        if (currentChanged && currentTrack != null)
        {
            AudioConverter converter = currentTrack.File.AudioConverter;
            _totalSamplesConv = converter.GetSampleCountInFormat(CurrentStreamingFormat);
            if (currentTrack.SetLoopingCurrent) LoopingCurrent = true;
            if (Status == PlaybackStatus.NotPlaying) Status = PlaybackStatus.Playing;
        }

        // if current changed at all.
        if (currentChanged)
        {
            OnTrackChanged?.Invoke(_currentTrack?.File, currentTrack?.File);
            _loopCount = 0;
            TrackChangedFx(currentTrack, prevTrackPlayHead);

            // Current changed, but we're not playing. If we don't
            // drop the cached audio then once we start playing a couple
            // of frames of the old current will leak.
            if (Status != PlaybackStatus.Playing) InvalidateAudioBlocks();
        }

        _currentTrack = currentTrack;
        _nextTrack = nextTrack;
    }

    private void BufferDataInAdvance(int framesToGet)
    {
        int bytesToGet = framesToGet * CurrentStreamingFormat.FrameSize;
        if (bytesToGet == 0) return;

        // Get a data block to fill. If we're over the maximum blocks this might mean overriding one that was ready.
        // This will happen if the backend is lagging behind, which should never happen?
        AudioDataBlock dataBlock;
        if (_readyBlocks.Count >= MaxDataBlocks && _readyBlocks.TryDequeue(out AudioDataBlock? b))
        {
#if DEBUG
            MetricBackendMissedFrames += b.FramesWritten - b.FramesRead;
#endif
            dataBlock = b;
        }
        else
        {
            dataBlock = _dataPool.Get();
        }

        // Ensure the data block can fit the requested data.
        // The data a data block will usually carry is around AudioUpdateRate * 2 in time.
        if (dataBlock.Data.Length < bytesToGet)
        {
#if DEBUG
            Interlocked.Add(ref MetricAllocatedDataBlocks, -dataBlock.Data.Length + bytesToGet); // metric shared between threads (layers)
#endif
            Array.Resize(ref dataBlock.Data, bytesToGet);
        }

        int framesGotten = GetDataToBuffer(framesToGet, new Span<byte>(dataBlock.Data, 0, bytesToGet));

        // Nothing streamed, track is probably over.
        if (framesGotten == 0)
        {
            _dataPool.Return(dataBlock);
        }
        else
        {
            // Reset data block trackers so it can be used.
            dataBlock.FramesWritten = framesGotten;
            dataBlock.FramesRead = 0;
            _readyBlocks.Enqueue(dataBlock);
        }
    }

    private void GoNextTrack(bool ignoreLoop = false)
    {
        // Check if looping.
        if (LoopingCurrent && !ignoreLoop)
        {
            // Manually update playhead as track wont change.
            int prevTrackPlayHead = _playHead;
            // Debug.Assert(prevTrackPlayHead == _totalSamplesConv); Not true when crossfading
            _playHead = 0;
            _loopCount++;
            OnTrackLoop?.Invoke(_currentTrack!.File);
            TrackChangedFx(_currentTrack!, prevTrackPlayHead);
        }
        // Otherwise, go to next track.
        else
        {
            lock (_playlist)
            {
                _playlist.Remove(_currentTrack!); // Don't remove by index as it is possible for current track to not be first via data race.
            }

            InvalidateCurrentTrack();
            UpdateCurrentTrack();
        }
    }

    protected int GetDataToBuffer(int framesRequested, Span<byte> dest, int framesOffset = 0)
    {
        // Check if the internal buffer needs to be resized to fit in the request.
        int samplesRequested = framesRequested * CurrentStreamingFormat.Channels;
        if (_internalBuffer.Length < samplesRequested)
        {
            Array.Resize(ref _internalBuffer, samplesRequested);
            Array.Resize(ref _internalBufferCrossFade, samplesRequested);
        }

#if DEBUG
        // Verify that the number of samples will fit in the buffer.
        // This should never happen and is considered a backend error.
        int bytesNeeded = framesRequested * CurrentStreamingFormat.FrameSize;
        if (dest.Length < bytesNeeded)
        {
            Engine.Log.Warning($"The provided buffer to the audio streamer is of invalid size {dest.Length} while {bytesNeeded} were requested.", MessageSource.Audio);
            framesRequested = dest.Length / CurrentStreamingFormat.FrameSize;
        }
#endif

        if (_currentTrack == null) return 0;

        // Get post processed 32f buffer data.
        int framesOutput = GetProcessedFramesFromTrack(CurrentStreamingFormat, _currentTrack, framesRequested, _internalBuffer, _playHead);
        _playHead += framesOutput * CurrentStreamingFormat.Channels;

        int destByteOffset = framesOffset * CurrentStreamingFormat.FrameSize;
        if (destByteOffset >= dest.Length)
        {
            Assert(false);
            Engine.Log.Error($"Frames dont fit in destination :/", MessageSource.Audio);
            return 0;
        }

        // Convert data to the destination sample size format.
        Span<byte> destBuffer = dest.Slice(framesOffset * CurrentStreamingFormat.FrameSize);
        var srcBuffer = new Span<float>(_internalBuffer, 0, samplesRequested);
        AudioHelpers.SetBufferOfSamplesAsFloat(srcBuffer, destBuffer, CurrentStreamingFormat);

        // Check if the buffer was filled.
        // If less frames were drawn than the buffer can take - the current track is over.
        if (framesOutput == framesRequested) return framesOutput;

        GoNextTrack();

        // Fill rest of buffer with samples from the next track (which is now the current track).
        if (_currentTrack != null) framesOutput += GetDataToBuffer(framesRequested - framesOutput, dest, framesOutput);

        return framesOutput;
    }

    protected abstract void UpdateBackend();

    protected int BackendGetData(AudioFormat format, int getFrames, Span<byte> buffer)
    {
        if (Status != PlaybackStatus.Playing) return 0;

        // Make sure we're getting the samples in the format we think we are.
        if (!format.Equals(CurrentStreamingFormat))
        {
            AudioFormat oldFormat = CurrentStreamingFormat;
            float progress = _playHead != 0 ? (float) _playHead / _totalSamplesConv : 0;
            CurrentStreamingFormat = format;

            if (format.UnsupportedBitsPerSample())
                Engine.Log.Warning($"Unsupported bits per sample format by streaming format - {CurrentStreamingFormat}", MessageSource.Audio, true);

            // Continue from last streaming position. This could cause a jump
            // depending on where the backend is.
            if (_currentTrack != null)
            {
                AudioConverter streamer = _currentTrack.File.AudioConverter;
                _totalSamplesConv = streamer.GetSampleCountInFormat(CurrentStreamingFormat);
                FormatChangedRecalculateFx(_currentTrack, oldFormat, format);
            }
            else
            {
                _totalSamplesConv = 0;
            }

            _playHead = (int) MathF.Floor(_totalSamplesConv * progress);
            InvalidateAudioBlocks();
        }

        // Verify dst size
        Assert(buffer.Length / format.FrameSize == getFrames);

        int framesLeft = getFrames;
        var framesGotten = 0;
        while (framesLeft > 0 && _readyBlocks.Count != 0)
        {
            if (_readyBlocks.TryPeek(out AudioDataBlock? b))
            {
                int framesGot = b.FramesWritten - b.FramesRead;
                int framesCanGet = Math.Min(framesGot, framesLeft);

                int bufferCopyOffset = b.FramesRead * format.FrameSize;
                int bufferCopyLength = framesCanGet * format.FrameSize;
                new Span<byte>(b.Data).Slice(bufferCopyOffset, bufferCopyLength).CopyTo(buffer);
                buffer = buffer[bufferCopyLength..];

                b.FramesRead += framesCanGet;
                framesGotten += framesCanGet;
                framesLeft -= framesCanGet;

                Assert(b.FramesRead <= b.FramesWritten);
                if (b.FramesRead == b.FramesWritten)
                {
                    _readyBlocks.TryDequeue(out AudioDataBlock? _);
                    _dataPool.Return(b);
                }
            }
        }

        // Buffer was starved, stream directly to it.
        // This occurs frequently when starting to play a new track as
        // the backend buffer requests data further into the future initially.
        if (framesLeft > 0 && _readyBlocks.Count == 0)
        {
#if DEBUG
            MetricStarved += framesLeft;
#endif

            int framesGot = GetDataToBuffer(framesLeft, buffer);
            framesGotten += framesGot;
        }

        return framesGotten;
    }

    protected void InvalidateAudioBlocks()
    {
        while (_readyBlocks.Count != 0 && _readyBlocks.TryDequeue(out AudioDataBlock? r))
            _dataPool.Return(r);

#if DEBUG
        MetricBackendMissedFrames = 0;
        MetricDataStoredInBlocks = 0;
#endif
    }

    public void SetPlayhead(float setTo)
    {
        lock (this)
        {
            if (_currentTrack == null) return;

            float progress = setTo / _currentTrack.File.Duration;
            _playHead = (int) Math.Clamp(_totalSamplesConv * progress, 0, _totalSamplesConv);
        }
    }

    #endregion

    public override string ToString()
    {
        return $"Layer {Name}, Playing: {CurrentTrack?.File.Name}";
    }

    public virtual void Dispose()
    {
        Disposed = true;
        InvalidateAudioBlocks();
    }
}