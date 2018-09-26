// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Graphics;
using Emotion.Primitives;
using Soul;

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
        public int Value { get; set; }

        #endregion

        public ScrollInput(Vector3 position, Vector2 size) : base(position, size)
        {
        }

        public override void Init()
        {
            Selector = new ScrollInputSelector(this, Vector3.Zero, Vector2.Zero);
            Controller.Add(Selector);

            OnResize += (a, b) => SyncChildren();
            OnMove += (a, b) => SyncChildren();
            SyncChildren();
        }

        public override void Draw(Renderer renderer)
        {
            // Sync active states.
            Selector.Active = Active;

            // Clamp value.
            Value = (int) MathHelper.Clamp(Value, 0, 100);

            // Draw bar.
            renderer.Render(Position, new Vector2(Width, Height), BarColor);

            // Calculate selector location.
            Selector.X = X + Width / 100 * Value - Selector.Width / 2;
        }

        private void SyncChildren()
        {
            Selector.Width = Width / 100 * 6;
            Selector.Height = (float) (Height + Height * 0.1 * 2);
            Selector.Center = Center;
            Selector.Z = Z + 1;
        }

        public override void Destroy()
        {
            Controller.Remove(Selector);
            base.Destroy();
        }
    }
}