// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Engine;
using Emotion.Game.UI;
using System.Numerics;

#endregion

namespace EmotionSandbox.SoundPlayer
{
    public class PlayerVolumeControl : ScrollInput
    {
        public PlayerVolumeControl(Vector3 position, Vector2 size) : base(position, size)
        {
            Value = Context.Settings.Volume;
        }

        protected override void ChangeValueFromClick(Vector2 clickPosition)
        {
            base.ChangeValueFromClick(clickPosition);
            Context.Settings.Volume = Value;
        }
    }
}