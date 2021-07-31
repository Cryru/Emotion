#region Using

using System;
using Emotion.Audio;
using Emotion.Standard.Audio;

#endregion

namespace Emotion.Platform.Implementation.Null
{
    public class NullAudioLayer : AudioLayer
    {
        public PlaybackStatus PreviousStatus { get; protected set; } = PlaybackStatus.NotPlaying;

        private AudioFormat _testFormat = new AudioFormat(32, true, 2, 48000);

        public NullAudioLayer(string name) : base(name)
        {
        }

        protected override void TransitionStatus(PlaybackStatus newStatus)
        {
            PreviousStatus = Status;
            base.TransitionStatus(newStatus);
        }

        public void AdvanceTime(int seconds)
        {
            var test = new Span<byte>(new byte[seconds * _testFormat.SampleRate * _testFormat.FrameSize]);
            GetDataForCurrentTrack(_testFormat, seconds * _testFormat.SampleRate, test);
        }

        public override void Dispose()
        {
        }
    }
}