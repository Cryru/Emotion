namespace Emotion.Standard.Audio
{
    public class AudioFormat
    {
        public int FrameSize
        {
            get => BitsPerSample / 8;
        }

        public int SampleSize
        {
            get => FrameSize * Channels;
        }

        public int BitsPerSample { get; set; }
        public bool IsFloat { get; set; }
        public int Channels { get; set; }
        public int SampleRate { get; set; }

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

        public bool Equals(AudioFormat f)
        {
            if(f == null) return false;
            return f.BitsPerSample == BitsPerSample && f.IsFloat == IsFloat && f.Channels == Channels && f.SampleRate == SampleRate;
        }

        public bool Equals(int bps, bool isFloat, int chan, int sampleRate)
        {
            return bps == BitsPerSample && isFloat == IsFloat && chan == Channels && sampleRate == SampleRate;
        }

        public AudioFormat Copy()
        {
            return new AudioFormat(BitsPerSample, IsFloat, Channels, SampleRate);
        }
    }
}