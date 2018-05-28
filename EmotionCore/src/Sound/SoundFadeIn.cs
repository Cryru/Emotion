// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Debug;

#endregion

namespace Emotion.Sound
{
    public class SoundFadeIn : SoundEffect
    {
        public override event EventHandler OnFinished;

        private double _volumeIncrement;

        public SoundFadeIn(float totalTime, Source source)
        {
            _time = totalTime / 10;
            RelatedSource = source;
            _volumeIncrement = RelatedSource.PersonalVolume / 10;
            RelatedSource.PersonalVolume = 0f;
        }

        internal override void Update(float frameTime)
        {
            if (Finished) return;

            _timeElapsed += frameTime;

            if (_timeElapsed > _time)
            {
                _iteration++;
                _timeElapsed -= _time;
                FadeCloser();
            }

            if (_iteration == 10)
            {
                OnFinished?.Invoke(this, EventArgs.Empty);
                Finished = true;
            }
        }

        private void FadeCloser()
        {
            Debugger.Log(MessageType.Trace, MessageSource.SoundManager, "Fade in effect of source " + RelatedSource.Pointer + " is at volume: " + _volumeIncrement * _iteration);
            RelatedSource.PersonalVolume = (float) (_volumeIncrement * _iteration);
        }
    }
}