#region Using

using System.Runtime.CompilerServices;
using Emotion.Standard.Audio;

#endregion

#nullable enable

namespace Emotion.Audio
{
    public abstract partial class AudioLayer
    {
        private float _baseVolumeLastApplied = -1;

        private bool _triggerCrossFadeToNext; // On the next tick start a crossfade into the next track.

        private bool _triggerFadeOutAndStop; // On the next tick start a fade out and then stop
        private List<AudioTrack>? _triggeredFadeOutStopPlaylist; // The tracks to remove from the playlist when ^ ends.

        private float _triggerEffectDuration = -1; // The duration of the trigger fade.

        // Whether the triggered crossfade is currently fading out rather than crossfading, because there is no current track.
        // It will start crossfading as soon as a track is added.
        // If this is too late it will sound weird tho.
        private bool _triggeredCrossFadeFadingOut;

        private AudioFormat _formatResample = new();

        protected int GetProcessedFramesFromTrack(AudioFormat format, AudioTrack track, int frames, float[] memory, int sampleStartOffset, bool crossFade = false)
        {
            if (track.PitchShiftFactor != 1 && track.PitchShiftFactor > 0)
            {
                format.CopyTo(_formatResample);
                format = _formatResample;
                format.SampleRate = (int) (format.SampleRate * track.PitchShiftFactor);
            }

            // Get data.
            int channels = format.Channels;
            int framesOutput = track.File.AudioConverter.GetResamplesFrames(format, sampleStartOffset, frames, memory);
            Assert(framesOutput <= frames);

            // Force mono post process.
            // Dont apply force mono on tracks while the resampler applied mono to.
            bool mergeChannels = Engine.Configuration.ForceMono && channels != 1 && track.File.Format.Channels != 1;
            if (mergeChannels) PostProcessConvertToMono(framesOutput, memory, channels);

            // If this is for the second cross fade track, we can skip the checks below.
            if (crossFade)
            {
                ApplyVolumeToFrames(_playHead, memory, framesOutput, _baseVolumeLastApplied, _baseVolumeLastApplied, true);
                return framesOutput;
            }

            float baseVolume = VolumeModifier * Engine.Configuration.MasterVolume;
            baseVolume = baseVolume * baseVolume;
            if (_baseVolumeLastApplied == -1) _baseVolumeLastApplied = baseVolume;

            float lastApplied = ApplyVolumeToFrames(sampleStartOffset, memory, framesOutput, baseVolume, _baseVolumeLastApplied);
            _baseVolumeLastApplied = lastApplied; // Record the base volume applied, as it is possible for the new one to not have applied in a single tick.

            int endSample = sampleStartOffset + framesOutput * channels;
            if (_fadeOutCrossFadeVol != null && _fadeOutCrossFadeVol.StartSample < endSample) // Normal track triggered cross fade
            {
                _fadeOutCrossFadeVol.SetCrossFadeProps(track, endSample);

                // Transfer effect from "out" of the previous track to the "in" of the new track.
                CrossFadeVolModEffect crossFadeEffect = _fadeOutCrossFadeVol;
                GoNextTrack();
                _fadeInCrossFadeVol = crossFadeEffect;
                _fadeInVol = null; // Remove fade in.
            }
            else if (_fadeInCrossFadeVol != null) // Cross fade in progress
            {
                AssertNotNull(_fadeInCrossFadeVol.Track);

                bool end = _fadeInCrossFadeVol.EndSample < endSample;
                var framesGotten = 0;
                var writeOffset = 0;
                while (!end && framesGotten < framesOutput)
                {
                    // Get the frames from the track we're cross fading from and merge them with the current.
                    // They'll be modulated to match volumes.
                    framesGotten = GetProcessedFramesFromTrack(format, _fadeInCrossFadeVol.Track, framesOutput, _internalBufferCrossFade, _fadeInCrossFadeVol.PlayHead, true);
                    int samplesGotten = framesGotten * channels;
                    _fadeInCrossFadeVol.PlayHead += samplesGotten;
                    for (var i = 0; i < samplesGotten; i++)
                    {
                        memory[writeOffset + i] += _internalBufferCrossFade[i];
                    }

                    writeOffset += samplesGotten;

                    // Cross fade track finished
                    if (framesGotten < framesOutput)
                    {
                        // If looping restart.
                        if (_fadeInCrossFadeVol.Looping)
                            _fadeInCrossFadeVol.PlayHead = 0;
                        else // If not lopping end cross fade. This might suck.
                            end = true;
                    }
                }

                if (end) _fadeInCrossFadeVol = null;
            }
            else if (_triggerFadeOutAndStop && _fadeOutTriggered == null) // Triggered fade and stop
            {
                int curTrackPlayHead = endSample;
                AudioTrack curTrack = track;

                // Calculate duration samples.
                float val = _triggerEffectDuration;
                if (val < 0) val = curTrack.File.Duration * -val;
                int sampleDur = format.SecondsToFrames(val) * format.Channels;

                _fadeOutTriggered = new VolumeModulationEffect(1, 0, curTrackPlayHead, curTrackPlayHead + sampleDur, EffectPosition.Absolute);
                _triggerFadeOutAndStop = false;
            }
            else if (_triggerCrossFadeToNext && _fadeOutTriggered == null) // Triggered cross fade into next track (ignore loop), cannot occur while a crossfade is ongoing.
            {
                int curTrackPlayHead = endSample;
                AudioTrack curTrack = track;

                // Calculate duration samples.
                float val = _triggerEffectDuration;
                if (val < 0) val = curTrack.File.Duration * -val;
                if (!LoopingCurrent) // if not looping, clamp duration to remaining.
                {
                    float played = (float) endSample / _totalSamplesConv;
                    float playedSeconds = curTrack.File.Duration * played;
                    float timeLeft = curTrack.File.Duration - playedSeconds;
                    if (val > timeLeft)
                        val = timeLeft;
                }

                int sampleDur = format.SecondsToFrames(val) * format.Channels;

                var effect = new CrossFadeVolModEffect(1, 0, curTrackPlayHead, curTrackPlayHead + sampleDur, EffectPosition.TrackRelative);

                // If there is a next non-loop track change to it.
                if (_nextTrack != null && _playlist.Count > 1)
                {
                    GoNextTrack(true);
                    effect.SetCrossFadeProps(track, curTrackPlayHead, LoopingCurrent);
                    effect.Pos = EffectPosition.TrackRelative;
                    _fadeInVol = null; // Remove fade in as it is going to be part of the crossfade.
                    _fadeInCrossFadeVol = effect;
                }
                else
                {
                    _triggeredCrossFadeFadingOut = true;
                    effect.Pos = EffectPosition.Absolute;
                    _fadeOutTriggered = effect;

                    // Remove track crossfade as it could interfere.
                    _fadeOutCrossFadeVol = null;
                }

                _triggerCrossFadeToNext = false;
            }
            else if (_fadeOutTriggered != null) // Performing a triggered fade out. Could be an actual tfo or cross fade with no next track.
            {
                // We ended out in a fade
                if (endSample >= _fadeOutTriggered.EndSample)
                {
                    if (_triggeredCrossFadeFadingOut)
                    {
                        // Fade to Next with no next track ended up just fading out.
                        // Go to the next track which would probably be nothing unless
                        // something was added in the very last tick.
                        GoNextTrack(true);
                    }
                    else
                    {
                        // Clear the playlist of the tracks that were in there when
                        // stop with fade was called. More tracks could've been added.
                        AssertNotNull(_triggeredFadeOutStopPlaylist);
                        lock (_playlist)
                        {
                            for (var i = 0; i < _triggeredFadeOutStopPlaylist.Count; i++)
                            {
                                AudioTrack trackToRemove = _triggeredFadeOutStopPlaylist[i];
                                _playlist.Remove(trackToRemove);
                            }
                        }

                        _triggeredFadeOutStopPlaylist.Clear();
                        InvalidateCurrentTrack();
                        UpdateCurrentTrack();
                    }

                    _fadeOutTriggered = null;
                }
                // A fade to next started out as just a fade out because there was no next track,
                // but not a track to crossfade to was added. Switch modes. Switching back is unsupported lol.
                else if (_triggeredCrossFadeFadingOut && _nextTrack != null && _playlist.Count > 1)
                {
                    var fadeOutTriggerAsCrossFade = _fadeOutTriggered as CrossFadeVolModEffect;
                    AssertNotNull(fadeOutTriggerAsCrossFade);
                    _fadeOutTriggered = null;

                    float curVol = fadeOutTriggerAsCrossFade.GetVolumeAt(endSample);
                    fadeOutTriggerAsCrossFade.SetCrossFadeProps(track, endSample, LoopingCurrent);
                    fadeOutTriggerAsCrossFade.StartVolume = curVol;

                    GoNextTrack(true);
                    _fadeInVol = null; // Remove fade in as it is going to be part of the crossfade.
                    _fadeInCrossFadeVol = fadeOutTriggerAsCrossFade;
                    fadeOutTriggerAsCrossFade.Pos = EffectPosition.TrackRelative;
                }
            }

            return framesOutput;
        }

        /// <summary>
        /// Convert multi-channel samples to mono.
        /// </summary>
        private static void PostProcessConvertToMono(int framesOutput, float[] soundData, int srcChannels)
        {
            // Fast path for stereo to mono.
            if (srcChannels == 2)
            {
                for (var i = 0; i < framesOutput; i++)
                {
                    int sampleLeft = i * srcChannels;
                    int sampleRight = i * srcChannels + 1;

                    float leftData = soundData[sampleLeft];
                    float rightData = soundData[sampleRight];
                    float combined = leftData + rightData;
                    soundData[sampleLeft] = combined;
                    soundData[sampleRight] = combined;
                }

                return;
            }

            for (var i = 0; i < framesOutput; i++)
            {
                int sampleIdx = i * srcChannels;
                float sampleAccum = 0;
                for (var c = 0; c < srcChannels; c++)
                {
                    float sample = soundData[sampleIdx + c];
                    sampleAccum += sample;
                }

                for (var c = 0; c < srcChannels; c++)
                {
                    soundData[sampleIdx + c] = sampleAccum;
                }
            }
        }

        // The interval in seconds to apply volume modulation in.
        // Calculating it for every frame is expensive, so we chunk it.
        // If this is too small it's a huge performance hit,
        // If it is too big though, it will cause discontinuity.
        public const float VOLUME_MODULATION_INTERVAL = 0.05f;

        private VolumeModulationEffect? _fadeInVol;
        private VolumeModulationEffect? _fadeOutVol;
        private CrossFadeVolModEffect? _fadeOutCrossFadeVol; // Effect which triggers cross fade to start at end
        private CrossFadeVolModEffect? _fadeInCrossFadeVol; // Current cross fade from previous track.
        private VolumeModulationEffect? _fadeOutTriggered; // Current cross fade from previous track.
        private VolumeModulationEffect? _userModifier;

        /// <summary>
        /// Get the layer volume at the specified sample index.
        /// Used for debugging.
        /// </summary>
        /// <param name="atSample"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetVolume(int atSample)
        {
            var mod = 1f;
            if (_fadeInVol != null) mod *= _fadeInVol.GetVolumeAt(atSample);
            if (_fadeOutVol != null) mod *= _fadeOutVol.GetVolumeAt(atSample);
            if (_userModifier != null) mod *= _userModifier.GetVolumeAt(atSample);
            if (_fadeOutTriggered != null) mod *= _fadeOutTriggered.GetVolumeAt(atSample);
            return AudioUtil.VolumeToMultiplier(mod);
        }

        private void FormatChangedRecalculateFx(AudioTrack track, AudioFormat oldFormat, AudioFormat newFormat)
        {
            _fadeInVol?.FormatChanged(track, oldFormat, newFormat);
            _fadeOutVol?.FormatChanged(track, oldFormat, newFormat);
            _fadeOutCrossFadeVol?.FormatChanged(track, oldFormat, newFormat);
            _userModifier?.FormatChanged(track, oldFormat, newFormat);
            _fadeOutTriggered?.FormatChanged(track, oldFormat, newFormat);
        }

        private void TrackChangedFx(AudioTrack? currentTrack, int prevTrackPlayHead)
        {
            _fadeInVol = null;
            _fadeOutVol = null;
            _fadeOutCrossFadeVol = null;
            _fadeInCrossFadeVol = null;

            // Triggered cross fade
            _triggerEffectDuration = -1;
            _triggerCrossFadeToNext = false;
            _triggerFadeOutAndStop = false;
            if (_fadeOutTriggered != null && (_fadeOutTriggered.Pos == EffectPosition.TrackRelative || prevTrackPlayHead == 0 || currentTrack == null))
            {
                _fadeOutTriggered = null;
                _triggeredCrossFadeFadingOut = false;
                _triggeredFadeOutStopPlaylist?.Clear();
            }

            if (currentTrack != null && currentTrack.FadeIn.HasValue && (!currentTrack.FadeInOnlyFirstLoop || _loopCount == 0))
            {
                AudioFormat format = CurrentStreamingFormat;

                // Calculate fade in samples.
                float val = currentTrack.FadeIn.Value;
                if (val < 0) val = currentTrack.File.Duration * -val;
                int fadeInSamples = format.SecondsToFrames(val) * format.Channels;

                _fadeInVol = new VolumeModulationEffect(0f, 1f, 0, fadeInSamples, EffectPosition.TrackRelative);
            }

            if (currentTrack != null && (currentTrack.FadeOut.HasValue || currentTrack.CrossFade.HasValue))
            {
                AudioFormat format = CurrentStreamingFormat;

                // Calculate fade in samples.
                float val = currentTrack.CrossFade ?? currentTrack.FadeOut!.Value;
                if (val < 0) val = currentTrack.File.Duration * -val;
                val = currentTrack.File.Duration - val;
                int fadeOutSampleStart = format.SecondsToFrames(val) * format.Channels;

                if (currentTrack.CrossFade.HasValue)
                    _fadeOutCrossFadeVol = new CrossFadeVolModEffect(1f, 0f, fadeOutSampleStart, _totalSamplesConv, EffectPosition.TrackRelative);
                else
                    _fadeOutVol = new VolumeModulationEffect(1f, 0f, fadeOutSampleStart, _totalSamplesConv, EffectPosition.TrackRelative);
            }

            // Transition "absolute" positions fx.
            if (_userModifier != null) _userModifier.AlignAbsolutePosition(prevTrackPlayHead, _totalSamplesConv);
            if (_fadeOutTriggered != null && _fadeOutTriggered.Pos == EffectPosition.Absolute) _fadeOutTriggered.AlignAbsolutePosition(prevTrackPlayHead, _totalSamplesConv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float ApplyVolumeToFrames(int sampleStart, Span<float> frames, int framesInBuffer, float baseMod, float lastAppliedBaseMod, bool crossFade = false)
        {
            int channels = CurrentStreamingFormat.Channels;
            int samplesInBuffer = framesInBuffer * channels;

            int finalSample = sampleStart + samplesInBuffer;
            int intervalInSamples = CurrentStreamingFormat.SecondsToFrames(VOLUME_MODULATION_INTERVAL) * channels;

            var startingInterval = (int) MathF.Floor((float) sampleStart / intervalInSamples);
            var endingInterval = (int) MathF.Ceiling((float) finalSample / intervalInSamples);

            int firstZeroCrossIdx = -1;

            // If changing base mod, find a zero crossing on all channels at once,
            // to ensure no click is heard.
            if (baseMod != lastAppliedBaseMod)
            {
                Span<float> lastSampleValues = stackalloc float[channels];

                var crossZeroSampleIdx = 0;
                for (int i = startingInterval; i < endingInterval; i++)
                {
                    int sampleAmount = Math.Min(intervalInSamples, samplesInBuffer - crossZeroSampleIdx);
                    int sampleAmountStart = crossZeroSampleIdx;
                    int sampleEnd = sampleAmountStart + sampleAmount;

                    // Initialize last sample values.
                    if (i == startingInterval)
                        for (var c = 0; c < channels; c++)
                        {
                            lastSampleValues[c] = frames[sampleAmountStart + c];
                        }

                    for (int s = sampleAmountStart + channels; s < sampleEnd; s += channels)
                    {
                        var isZeroCrossing = true;
                        for (var c = 0; c < channels; c++)
                        {
                            float thisSample = frames[s + c];
                            float lastSample = lastSampleValues[c];

                            // Where the sound sine wave crosses 0 (-1 to 1 or vice versa) you
                            // can safely apply gain without causing clicks.
                            bool zeroCrossingOnThisChannel = thisSample * lastSample <= 0;
                            isZeroCrossing = isZeroCrossing && zeroCrossingOnThisChannel;
                            lastSampleValues[c] = thisSample;
                        }

                        if (isZeroCrossing)
                        {
                            firstZeroCrossIdx = s;
                            break;
                        }
                    }

                    if (firstZeroCrossIdx != -1) break;
                    crossZeroSampleIdx += sampleAmount;
                }
            }

            var sampleIdx = 0;
            for (int i = startingInterval; i < endingInterval; i++)
            {
                int sampleAmount = Math.Min(intervalInSamples, samplesInBuffer - sampleIdx);
                int sampleAmountStart = sampleIdx;
                int sampleEnd = sampleAmountStart + sampleAmount;

                int sampleIdxOfInterval = i * intervalInSamples;

                float intervalMod;
                if (crossFade)
                {
                    // In the cross fade track apply just the user modifier and reverse the cross fade modifier (as this is the track fading out)
                    intervalMod = 1f;
                    if (_userModifier != null) intervalMod *= AudioUtil.VolumeToMultiplier(_userModifier.GetVolumeAt(sampleIdxOfInterval));
                    if (_fadeInCrossFadeVol != null) intervalMod *= 1.0f - _fadeInCrossFadeVol.GetVolumeAt(sampleIdxOfInterval);
                }
                else
                {
                    intervalMod = GetVolume(sampleIdxOfInterval);
                    if (_fadeInCrossFadeVol != null) intervalMod *= _fadeInCrossFadeVol.GetVolumeAt(sampleIdxOfInterval);
                }

                float combinedMod = lastAppliedBaseMod * intervalMod;

                // Apply old gain up to crossing and then switch.
                if (firstZeroCrossIdx != -1 && firstZeroCrossIdx >= sampleAmountStart && firstZeroCrossIdx < sampleEnd)
                {
                    for (int s = sampleAmountStart; s < firstZeroCrossIdx; s++)
                    {
                        frames[s] *= combinedMod;
                    }

                    lastAppliedBaseMod = baseMod;
                    combinedMod = lastAppliedBaseMod * intervalMod;
                    sampleAmountStart = firstZeroCrossIdx;
                }

                // Apply volume to samples of interval.
                for (int s = sampleAmountStart; s < sampleEnd; s++)
                {
                    frames[s] *= combinedMod;
                }

                _debugLastAppliedSampleMod = combinedMod;
                sampleIdx += sampleAmount;
            }

            return lastAppliedBaseMod;
        }

        private float _debugLastAppliedSampleMod;

        /// <summary>
        /// Get the volume at the current playhead position.
        /// Use for debugging and visualization.
        /// </summary>
        public float DebugGetCurrentVolume()
        {
            return MathF.Round(_debugLastAppliedSampleMod, 2);
        }

        public void DebugSetPlaybackAtSecond(float second)
        {
            _playHead = CurrentStreamingFormat.SecondsToFrames(second) * CurrentStreamingFormat.Channels;
        }
    }
}