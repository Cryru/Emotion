// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Drawing;
using Emotion.Engine.Hosting.Desktop;

#endregion

namespace Emotion.Engine.Configuration
{
    /// <summary>
    /// Settings pertaining to audio functionality.
    /// </summary>
    public class SoundSettings
    {
        /// <summary>
        /// Whether to play sound.
        /// </summary>
        public bool Sound { get; set; } = true;

        /// <summary>
        /// The volume to play sound at. From 0 to 100.
        /// </summary>
        public int Volume { get; set; } = 100;
    }
}