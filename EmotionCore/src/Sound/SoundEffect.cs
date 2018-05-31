// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;

#endregion

namespace Emotion.Sound
{
    public abstract class SoundEffect
    {
        /// <summary>
        /// Whether the effect is considered finished.
        /// </summary>
        public bool Finished { get; protected set; }

        /// <summary>
        /// The function to invoke when the effect is finished.
        /// </summary>
        public abstract event EventHandler OnFinished;

        /// <summary>
        /// The source the effect is pertaining to.
        /// </summary>
        public SoundLayer RelatedLayer { get; protected set; }

        protected float _time;
        protected float _timeElapsed;
        protected float _iteration;

        internal abstract void Update(float frameTime);

        #region Debugging

        public override string ToString()
        {
            string result = "[" + base.ToString() + "]";
            result += $"(source: {RelatedLayer.Source})";
            return result;
        }

        #endregion
    }
}