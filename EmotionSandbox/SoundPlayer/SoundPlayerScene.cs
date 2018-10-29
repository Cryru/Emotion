using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emotion.Game.Layering;
using Emotion.Graphics;
using Emotion.Engine;
using EmotionSandbox.Examples.Generic;

namespace EmotionSandbox.SoundPlayer
{
    public class SoundPlayerScene : Layer
    {
        public static void Main()
        {
            Context.Setup();

            Context.LayerManager.Add(new LoadingScreen(), "__loading__", 0);
            Context.LayerManager.Add(new SoundPlayerScene(), "soundPlayer", 1);
        }

        public override void Load()
        {
            throw new NotImplementedException();
        }

        public override void Update(float frameTime)
        {
            throw new NotImplementedException();
        }

        public override void Draw(Renderer renderer)
        {
            throw new NotImplementedException();
        }

        public override void Unload()
        {
            throw new NotImplementedException();
        }
    }
}
