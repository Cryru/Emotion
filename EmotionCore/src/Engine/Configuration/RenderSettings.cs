// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Numerics;
using Emotion.Primitives;

#endregion

namespace Emotion.Engine.Configuration
{
    /// <summary>
    /// Settings pertaining to rendering.
    /// </summary>
    public class RenderSettings
    {
        /// <summary>
        /// The color to clear the window with.
        /// </summary>
        public Color ClearColor = Color.CornflowerBlue;

        /// <summary>
        /// The render size as a vector2. Contains RenderWidth and RenderHeight.
        /// </summary>
        public Vector2 Size
        {
            get => new Vector2(Width, Height);
            set
            {
                Width = (int) value.X;
                Height = (int) value.Y;
            }
        }

        /// <summary>
        /// The width to render at.
        /// </summary>
        public float Width = 960;

        /// <summary>
        /// The height to render at.
        /// </summary>
        public float Height = 540;

        /// <summary>
        /// The maximum fps to render at. Set to 0 if uncapped. VSync and other settings might still cap it.
        /// </summary>
        public int CapFPS = 60;
    }
}