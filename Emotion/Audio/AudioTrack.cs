#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// The layer the track is playing on.
        /// </summary>
        public AudioLayer Layer { get; set; }

        /// <summary>
        /// How far along the duration of the file the track has finished playing.
        /// </summary>
        public float Playback
        {
            get => Progress * File.Duration;
        }

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

        public override float GetSampleAsFloat(int sampleIdx, bool trueIndex = false, bool secondChannel = false)
        {
            float volume = Layer.Volume * Engine.Configuration.MasterVolume;
            float sample = base.GetSampleAsFloat(sampleIdx, trueIndex, secondChannel);

            if (_playbackEvents != null)
            {
                float progress = (float) sampleIdx / _sourceConvLength;
                float playback = progress * File.Duration;
                Debug.Assert(progress >= 0.0f && progress <= 1.0f);
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