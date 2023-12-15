#region Using

using System.Collections.ObjectModel;
using Emotion.Standard.TMX;

#endregion

namespace Emotion.Game.Tiled
{
    /// <summary>
    /// An internal object to be used by the map object to animate tiles.
    /// Holds the current state of a tmx animated tile.
    /// </summary>
    public class AnimatedTile
    {
        #region Properties

        /// <summary>
        /// The gid of the animated tile within its tileset.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// The gid of the current frame.
        /// </summary>
        public int FrameId
        {
            get => _frames[_currentFrame].Id;
        }

        #endregion

        private float _timePassed;
        private int _currentFrame;
        private Collection<TmxAnimationFrame> _frames;

        /// <summary>
        /// Meta data for an animated tile map tile.
        /// </summary>
        /// <param name="gid">The global id of the tile.</param>
        /// <param name="frames">The animation frames of the tile.</param>
        public AnimatedTile(int gid, Collection<TmxAnimationFrame> frames)
        {
            Id = gid;
            _frames = frames;
        }

        /// <summary>
        /// Progress time for the animated tile.
        /// </summary>
        /// <param name="time">The amount of time passed since the last update.</param>
        /// <returns>Whether an update was triggered.</returns>
        public bool Update(float time)
        {
            _timePassed += time;
            var wasUpdate = false;

            // Check if pass the duration of the current frame.
            while (_timePassed >= _frames[_currentFrame].Duration)
            {
                wasUpdate = true;

                // Subtract time and increment frame.
                _timePassed -= _frames[_currentFrame].Duration;
                _currentFrame++;

                // Check if overflowing.
                if (_currentFrame >= _frames.Count) _currentFrame = 0;
            }

            return wasUpdate;
        }
    }
}