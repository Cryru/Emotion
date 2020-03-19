﻿#region Using

using System;
using Emotion.Common;
using Emotion.Standard.Audio;
using Emotion.Standard.Audio.WAV;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.IO
{
    /// <inheritdoc />
    /// <summary>
    /// A sound file asset. Supports WAV.
    /// </summary>
    public class AudioAsset : Asset
    {
        #region Properties

        /// <summary>
        /// The duration of the sound file, in seconds.
        /// </summary>
        public float Duration { get; private set; }

        /// <summary>
        /// The sound data itself.
        /// </summary>
        public Memory<byte> SoundData { get; private set; }

        /// <summary>
        /// The sound format.
        /// </summary>
        public AudioFormat Format { get; private set; }

        #endregion

        protected override void CreateInternal(byte[] data)
        {
            // Check if WAV.
            if (WavFormat.IsWav(data))
            {
                SoundData = WavFormat.Decode(data, out AudioFormat format);
                Format = format;
                Duration = format.GetSoundDuration(data.Length);
            }

            if (Format == null || SoundData.IsEmpty) Engine.Log.Warning($"Couldn't load audio file - {Name}.", MessageSource.AssetLoader);
        }

        protected override void DisposeInternal()
        {
            SoundData = null;
            Format = null;
        }
    }
}