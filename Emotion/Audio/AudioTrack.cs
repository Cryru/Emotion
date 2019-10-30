#region Using

using Emotion.IO;
using Emotion.Standard.Audio;

#endregion

namespace Emotion.Audio
{
    public class AudioTrack
    {
        public AudioStreamer Streamer { get; }

        public AudioAsset File { get; set; }

        public AudioTrack(AudioAsset file)
        {
            File = file;
            Streamer = new AudioStreamer(File.Format, File.SoundData);
        }
    }
}