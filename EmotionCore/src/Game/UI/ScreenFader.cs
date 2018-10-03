// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.System;

#endregion

namespace Emotion.Game.UI
{
    public sealed class ScreenFader
    {
        /// <summary>
        /// Whether the fade effect is complete.
        /// </summary>
        public bool Done { get; private set; } = true;

        /// <summary>
        /// The color of the fader.
        /// </summary>
        public Color Color { get; set; } = Color.White;

        private float _fadeDuration;
        private float _fadeTimer;
        private int _opacity;
        private FadeDirection _direction;

        /// <summary>
        /// Create a new screen fader object.
        /// </summary>
        /// <param name="startingOpacity">The opacity to start at. 0 is invisible and 255 is opaque.</param>
        public ScreenFader(int startingOpacity = 0)
        {
            _opacity = startingOpacity;
        }

        /// <summary>
        /// Update the fader, moving it in time.
        /// </summary>
        /// <param name="renderTime">The amount of time in milliseconds which have passed since the last update.</param>
        public void Update(float renderTime)
        {
            // Check if done.
            if (Done) return;

            // Add the time passed.
            _fadeTimer += renderTime;

            // Check if enough time has passed.
            if (!(_fadeTimer > _fadeDuration)) return;

            // Subtract from the timer.
            _fadeTimer -= _fadeDuration;

            // Move opacity based on direction.
            switch (_direction)
            {
                case FadeDirection.In:
                    _opacity += 1;
                    if (_opacity == 255) Done = true;
                    break;
                case FadeDirection.Out:
                    _opacity -= 1;
                    if (_opacity == 0) Done = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Draw the screen fader. Is drawn over the whole screen with the opacity currently set.
        /// </summary>
        /// <param name="renderer">The renderer to use.</param>
        public void Draw(Renderer renderer)
        {
            Color addOpacity = Color;
            addOpacity.A = (byte) _opacity;

            renderer.DisableViewMatrix();
            renderer.Render(new Vector3(0, 0, 0), new Vector2(Context.Settings.RenderWidth, Context.Settings.RenderHeight), addOpacity);
            renderer.EnableViewMatrix();
        }

        /// <summary>
        /// Start a new fade effect.
        /// </summary>
        /// <param name="time">The time in milliseconds the fade should take to complete.</param>
        /// <param name="direction">The direction of the fade, either fading in or out.</param>
        public void Set(int time, FadeDirection direction)
        {
            _fadeDuration = time / 255;
            _direction = direction;
            _fadeTimer = 0;
            Done = false;

            // Set starting opacity based on direction.
            switch (_direction)
            {
                case FadeDirection.In:
                    _opacity = 0;
                    break;
                case FadeDirection.Out:
                    _opacity = 255;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Instantly end the fade.
        /// </summary>
        public void End()
        {
            Done = true;

            // Set ending opacity based on direction.
            switch (_direction)
            {
                case FadeDirection.In:
                    _opacity = 255;
                    break;
                case FadeDirection.Out:
                    _opacity = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum FadeDirection
    {
        In,
        Out
    }
}