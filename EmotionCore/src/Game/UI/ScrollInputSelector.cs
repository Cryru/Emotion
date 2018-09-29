// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Graphics;
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
        private ScrollInput _scrollBar;

        public ScrollInputSelector(ScrollInput parent, Vector3 position, Vector2 size) : base(position, size)
        {
            _scrollBar = parent;
        }

        public override void Render(Renderer renderer)
        {
            // Render the selector according to whether it is held or not.
            renderer.Render(Vector3.Zero, Size, Held[0] ? HeldColor : Color);
        }

        public override void MouseEnter(Vector2 mousePosition)
        {
            _scrollBar.MouseEnter(mousePosition);
        }

        public override void MouseMoved(Vector2 oldPosition, Vector2 newPosition)
        {
            _scrollBar.MouseMoved(oldPosition, newPosition);
        }
    }
}