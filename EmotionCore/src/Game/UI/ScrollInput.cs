// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.GLES;
using Emotion.Primitives;
using Emotion.Utils;

#endregion

namespace Emotion.Game.UI
{
    public class ScrollInput : Control
    {
        #region Properties

        /// <summary>
        /// The color of the bar.
        /// </summary>
        public Color BarColor = Color.Black;

        /// <summary>
        /// The scroll input's selector.
        /// </summary>
        public ScrollInputSelector Selector;

        /// <summary>
        /// The current value of the input.
        /// </summary>
        public int Value { get; set; } = 0;

        #endregion

        public ScrollInput(Controller controller, Rectangle bounds, int priority) : base(controller, bounds, priority)
        {
            Selector = new ScrollInputSelector(this, controller, new Rectangle(), priority + 1);
        }

        public override void Draw(Renderer renderer)
        {
            // Sync active states.
            Selector.Active = Active;

            // Clamp value.
            Value = (int) Soul.MathHelper.Clamp(Value, 0, 100);

            // Draw bar.
            renderer.DrawRectangle(Bounds, BarColor, false);

            // Calculate selector.
            Rectangle selectorBounds = new Rectangle
            {
                Width = Bounds.Width / 100 * 6,
                Height = (int) (Bounds.Height + Bounds.Height * 0.1 * 2),
                Center = Bounds.Center
            };
            selectorBounds.X = Bounds.X + Bounds.Width / 100 * Value - selectorBounds.Width / 2;
            Selector.Bounds = selectorBounds;
        }

        public override void Destroy()
        {
            Selector.Destroy();
            base.Destroy();
        }
    }
}