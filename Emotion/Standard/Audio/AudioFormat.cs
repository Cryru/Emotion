namespace Emotion.Standard.Audio
{
    public class AudioFormat
    {
        /// <summary>
        /// How big a single sample is.
        /// A sample is the sound data per channel.
        /// </summary>
        public int SampleSize
        {
            get => BitsPerSample / 8;
        }

        /// <summary>
        /// How big a single frame is.
        /// A frame is made up of a sample for each channel.
        /// </summary>
        public int FrameSize
        {
            get => SampleSize * Channels;
        }

        /// <summary>
        /// How many bits a sample is.
        /// </summary>
        public int BitsPerSample { get; set; }

        /// <summary>
        /// Whether the format is a float. This is needed because both float and int32 are 32 bit.
        /// </summary>
        public bool IsFloat { get; set; }

        /// <summary>
        /// How many channels the format is.
        /// </summary>
        public int Channels { get; set; }

        /// <summary>
        /// How many samples are per second.
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        /// The default format - 32 bit Float Stereo 48k
        /// </summary>
        public AudioFormat()
        {
            BitsPerSample = 32;
            IsFloat = true;
            Channels = 2;
            SampleRate = 48000;
        }

        public AudioFormat(int bps, bool isFloat, int chan, int sampleRate)
        {
            BitsPerSample = bps;
            IsFloat = isFloat;
            Channels = chan;
            SampleRate = sampleRate;
        }

        /// <summary>
        /// Whether two format objects are the same format.
        /// </summary>
        public bool Equals(AudioFormat f)
        {
            if (f == null) return false;
            return f.BitsPerSample == BitsPerSample && f.IsFloat == IsFloat && f.Channels == Channels && f.SampleRate == SampleRate;
        }

        /// <summary>
        /// Whether the format object is of the specified format.
        /// </summary>
        public bool Equals(int bps, bool isFloat, int chan, int sampleRate)
        {
            return bps == BitsPerSample && isFloat == IsFloat && chan == Channels && sampleRate == SampleRate;
        }

        /// <summary>
        /// Create a copy of the format.
        /// </summary>
        public AudioFormat Copy()
        {
            return new AudioFormat(BitsPerSample, IsFloat, Channels, SampleRate);
        }

        /// <summary>
        /// Returns the duration of a sound in seconds based on it's sound data - if it is in this format.
        /// </summary>
        /// <param name="soundBufferBytes">The length of the sound buffer in bytes to find the duration of.</param>
        /// <returns>How long that buffer is in seconds.</returns>
        public float GetSoundDuration(int soundBufferBytes)
        {
            return (float) soundBufferBytes / (SampleRate * Channels * SampleSize);
        }

        public int GetFrameIndexAtTimestamp(float timeSeconds)
        {
            return (int) (timeSeconds * SampleRate * Channels);
        }

        /// <summary>
        /// Whether the format is of an unsupported bits per sample format.
        /// </summary>
        /// <returns></returns>
        public bool UnsupportedBitsPerSample()
        {
            int sampleSize = BitsPerSample;
            return sampleSize != 8 && sampleSize != 16 && sampleSize != 32;
        }
    }
}