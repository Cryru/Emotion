// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.GLES;
using Emotion.Input;
using Emotion.Primitives;
using Soul;

#endregion

namespace Emotion.Game.UI
{
    public sealed class ScrollInput : Control
    {
        #region Properties

        /// <summary>
        /// The color of the bar.
        /// </summary>
        public Color BarColor = Color.Black;

        /// <summary>
        /// The scroll input's selector.
        /// </summary>
        public ScrollInputSelector Selector { get; private set; }

        /// <summary>
        /// The current value of the input.
        /// </summary>
        public int Value { get; set; }

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
            Value = (int) MathHelper.Clamp(Value, 0, 100);

            // Draw bar.
            // todo
            //renderer.DrawRectangle(Bounds, BarColor, false);

            // Calculate selector position.
            Selector.Width = Width / 100 * 6;
            Selector.Height = (int) (Height + Height * 0.1 * 2);
            Selector.Center = Center;
            Selector.X = X + Width / 100 * Value - Selector.Width / 2;;
        }

        public override void Destroy()
        {
            Selector.Destroy();
            base.Destroy();
        }
    }
}