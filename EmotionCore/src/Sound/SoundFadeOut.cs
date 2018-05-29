// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Debug;

#endregion

namespace Emotion.Sound
{
    public class SoundFadeOut : SoundEffect
    {
        public override event EventHandler OnFinished;

        private double _personalVolumeStart;
        private double _volumeIncrement;

        public SoundFadeOut(float totalTime, Source source)
        {
            _time = totalTime / 10;
            RelatedSource = source;
            _personalVolumeStart = RelatedSource.PersonalVolume;
            _volumeIncrement = _personalVolumeStart / 10;
        }

        internal override void Update(float frameTime)
        {
            if (Finished) return;

            _timeElapsed += frameTime;

            if (_timeElapsed > _time)
            {
                _iteration++;
                _timeElapsed -= _time;
                FadeFurther();
            }

            if (_iteration == 10)
            {
                OnFinished?.Invoke(this, EventArgs.Empty);
                Finished = true;
            }
        }

        private void FadeFurther()
        {
            Debugger.Log(MessageType.Trace, MessageSource.SoundManager,
                "Fade out effect of source " + RelatedSource.Pointer + " is at volume: " + (_personalVolumeStart - _volumeIncrement * _iteration));
            RelatedSource.PersonalVolume = (float) (_personalVolumeStart - _volumeIncrement * _iteration);
        }
    }
}