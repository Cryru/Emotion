// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion;
using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.GLES;
using Emotion.Input;

#endregion

namespace EmotionSandbox.Examples
{
    public class Input : Layer
    {
        public static void Main()
        {
            Context context = Starter.GetEmotionContext();

            context.LayerManager.Add(new Input(), "Input Example", 0);

            context.Start();
        }

        public override void Load()
        {
        }

        public override void Draw(Renderer renderer)
        {
        }

        public override void Update(float frameTime)
        {
            if (Context.Input.IsMouseKeyDown(MouseKeys.Left)) Console.WriteLine("Mouse Left Click Down!");
            if (Context.Input.IsMouseKeyHeld(MouseKeys.Left)) Console.WriteLine("Mouse Left Click Held!");
        }

        public override void Unload()
        {
        }
    }
}