#region Using

using System;
using System.Collections.Generic;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Audio;

#endregion

namespace Emotion.Audio
{
    public class AudioTrack : AudioStreamer
    {
        /// <summary>
        /// The audio file this track is playing.
        /// </summary>
        public AudioAsset File { get; set; }

        /// <summary>
        /// Whether the track is playing on a layer.
        /// </summary>
        public bool HasLayer { get; set; }

        /// <summary>
        /// How far along the duration of the file the track has finished playing.
        /// </summary>
        public float Playback
        {
            get => Progress * File.Duration;
        }

        private float _volume = 1f;
        private List<AudioModulationEffect> _playbackEvents;

        public AudioTrack(AudioAsset file) : base(file.Format, file.SoundData)
        {
            File = file;
        }

        /// <summary>
        /// Add a new event to the track's playback.
        /// </summary>
        /// <param name="newEvent">The event to add.</param>
        public void AddAudioModulation(AudioModulationEffect newEvent)
        {
            if (_playbackEvents == null) _playbackEvents = new List<AudioModulationEffect>();
            _playbackEvents.Add(newEvent);
        }

        public int GetNextVolumeModulatedFrames(float volume, int frameCount, Span<byte> buffer)
        {
            // The rest of this is in GetSampleAsFloat and volume is kept as a member
            // due to the complexity of the resampling process and the modulation working with float values.
            _volume = volume;
            if (frameCount == 0) return 0;
            return base.GetNextFrames(frameCount, buffer);
        }

        public override float GetSampleAsFloat(int sampleIdx, bool trueIndex = false, bool secondChannel = false)
        {
            float volume = _volume;
            float sample = base.GetSampleAsFloat(sampleIdx, trueIndex, secondChannel);

            if (_playbackEvents != null)
            {
                int channels = ConvFormat?.Channels ?? SourceFormat.Channels;
                float progress = (float) sampleIdx / _sourceConvLength * channels;
                float playback = progress * File.Duration;
                for (var i = 0; i < _playbackEvents.Count; i++)
                {
                    _playbackEvents[i].Apply(ref sample, ref volume, progress, playback, this);
                }
            }

            volume = MathF.Pow(volume, Engine.Configuration.AudioCurve);
            sample *= volume;
            return sample;
        }
    }
}