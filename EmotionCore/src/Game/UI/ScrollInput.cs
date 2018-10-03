// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Graphics;
using Emotion.Input;
using Emotion.Primitives;
using Emotion.System;
using Soul;

#endregion

namespace Emotion.Game.UI
{
    public class ScrollInput : ParentControl
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
            Selector = new ScrollInputSelector(this, new Vector3(0, 0, 1), Vector2.Zero);
            SyncChildren();
            Selector.Center = CenterRelative;
        }

        public override void Init()
        {
            AddChild(Selector);
            OnResize += (a, b) => SyncChildren();
        }

        public override void OnActivate()
        {
            Selector.Active = true;
        }

        public override void OnDeactivate()
        {
            Selector.Active = false;
        }

        public override void MouseDown(MouseKeys key)
        {
            ChangeValue(Context.InputManager.GetMousePosition());
        }

        public override void MouseMoved(Vector2 oldPosition, Vector2 newPosition)
        {
            // Check if the bar or selector is held with the left click.
            if (!Held[0] && !Selector.Held[0]) return;

            ChangeValue(newPosition);
        }

        public override void Render(Renderer renderer)
        {
            // Clamp value.
            Value = (int) MathHelper.Clamp(Value, 0, 100);

            // Draw bar.
            renderer.Render(Vector3.Zero, Size, BarColor);

            // Calculate selector location.
            Selector.X = Width / 100 * Value - Selector.Width / 2;

            // Render child.
            base.Render(renderer);
        }

        public virtual void ChangeValue(Vector2 clickPosition)
        {
            // Calculate the new value form the new mouse position.
            float posWithinParent = clickPosition.X - GetTruePosition().X;
            float increment = Width / 100;
            Value = (int) (posWithinParent / increment);
        }

        protected virtual void SyncChildren()
        {
            Selector.Width = Width / 100 * 6;
            Selector.Height = (float) (Height + Height * 0.1 * 2);
        }
    }
}