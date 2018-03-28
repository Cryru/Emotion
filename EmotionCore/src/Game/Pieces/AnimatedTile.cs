﻿// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.ObjectModel;
using TiledSharp;

#endregion

namespace Emotion.Game.Pieces
{
    /// <summary>
    /// An internal object to be used by the map object to animate tiles.
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

        public AnimatedTile(int gid, Collection<TmxAnimationFrame> frames)
        {
            Id = gid;

            _frames = frames;
        }

        /// <summary>
        /// Progress time for the animated tile.
        /// </summary>
        /// <param name="time">The amount of time passed since the last update.</param>
        public void Update(float time)
        {
            _timePassed += time;

            // Check if pass the duration of the current frame.
            if (!(_frames[_currentFrame].Duration <= _timePassed)) return;

            // Subtract time and increment frame.
            _timePassed -= _frames[_currentFrame].Duration;
            _currentFrame++;

            // Check if overflowing.
            if (_currentFrame >= _frames.Count) _currentFrame = 0;
        }
    }
}