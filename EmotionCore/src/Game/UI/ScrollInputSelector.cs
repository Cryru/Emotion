// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.GLES;
using Emotion.Input;
using Emotion.IO;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.UI
{
    public class ScrollInputSelector : Control
    {
        #region Properties

        /// <summary>
        /// The color of the selector.
        /// </summary>
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// The color of the selector when held.
        /// </summary>
        public Color HeldColor { get; set; } = Color.Red;

        #endregion

        /// <summary>
        /// The parent scroll input.
        /// </summary>
        private ScrollInput _parent;

        private SoundFile _scrollSound;

        public ScrollInputSelector(ScrollInput parent, Controller controller, Rectangle bounds, int priority) : base(controller, bounds, priority)
        {
            _parent = parent;
            _scrollSound = _controller.Context.AssetLoader.Get<SoundFile>("Music/UI/slider_short.wav");
        }

        public override void Draw(Renderer renderer)
        {
            // Sync active states.
            Active = _parent.Active;

            // Render the selector according to whether it is held or not.
            renderer.DrawRectangle(Bounds, Held[0] ? HeldColor : Color, false);
        }

        public override void MouseMoved(Vector2 oldPosition, Vector2 newPosition)
        {
            // Check if held with the left click.
            if (!Held[0]) return;

            // Calculate the new value form the new mouse position.
            float posWithinParent = newPosition.X - _parent.Bounds.X;
            float increment = _parent.Bounds.Width / 100;
            _parent.Value = (int) (posWithinParent / increment);

            if (_controller.Context.SoundManager.GetLayer("UI").Source.Paused)
            {
                _controller.Context.SoundManager.GetLayer("UI").Source.Play();
            }
        }

        public override void MouseDown(MouseKeys key)
        {
            _controller.Context.SoundManager.PlayOnLayer("UI", _scrollSound);
            _controller.Context.SoundManager.GetLayer("UI").Source.Looping = true;
        }

        public override void MouseUp(MouseKeys key)
        {
            _controller.Context.SoundManager.GetLayer("UI").Source.Destroy();
        }
    }
}