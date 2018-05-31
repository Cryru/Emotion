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

        public SoundFadeIn(float totalTime, SoundLayer source)
        {
            _time = totalTime / 10;
            RelatedLayer = source;
            _volumeIncrement = RelatedLayer.Volume / 10;
            RelatedLayer.Volume = 0f;
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
            if (RelatedLayer.SourceDestroyed) Finished = true;

            Debugger.Log(MessageType.Trace, MessageSource.SoundManager, "Fade in effect of layer " + RelatedLayer + " is at volume: " + _volumeIncrement * _iteration);
            RelatedLayer.Volume = (float) (_volumeIncrement * _iteration);
        }
    }
}