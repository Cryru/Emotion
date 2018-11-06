// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Game.UI;
using Emotion.Graphics.Text;
using Emotion.Input;
using Emotion.Primitives;

#endregion

namespace EmotionSandbox.SoundPlayer
{
    public class SoundSelectionLabel : BasicText
    {
        public Action OnClick { get; set; }

        public SoundSelectionLabel(Font font, uint textSize, string text, Color color, Vector3 position) : base(font, textSize, text, color, position)
        {
        }

        public override void MouseDown(MouseKeys key)
        {
            OnClick?.Invoke();
        }
    }
}