// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Graphics;
using Emotion.Graphics.GLES;
using Emotion.Input;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.UI
{
    public class BasicButton : Control
    {
        /// <summary>
        /// The texture of the button.
        /// </summary>
        public Texture Texture { get; set; }

        /// <summary>
        /// The event to trigger when the button is clicked.
        /// </summary>
        public Action OnClick { get; set; }

        public BasicButton(Vector3 position, Vector2 size) : base(position, size)
        {
        }

        public override void MouseDown(MouseKeys key)
        {
            OnClick?.Invoke();
        }

        public override void Render(Renderer renderer)
        {
            renderer.Render(Vector3.Zero, Size, Color.White, Texture);
        }
    }
}