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
        private byte[] _testArray = new byte[1];

        public NullAudioLayer(string name) : base(name)
        {
        }

        protected override void TransitionStatus(PlaybackStatus newStatus)
        {
            PreviousStatus = Status;
            base.TransitionStatus(newStatus);
        }

        protected override void UpdateBackend()
        {

        }

        public void AdvanceTime(int seconds)
        {
            int sizeNeeded = seconds * _testFormat.SampleRate * _testFormat.FrameSize;
            if (_testArray.Length < sizeNeeded) Array.Resize(ref _testArray, sizeNeeded);
            BackendGetData(_testFormat, seconds * _testFormat.SampleRate, _testArray);
        }

        public override void Dispose()
        {
        }
    }
}