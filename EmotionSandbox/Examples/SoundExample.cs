// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.GLES;
using Emotion.IO;
using Emotion.Sound;

#endregion

namespace EmotionSandbox.Examples
{
    public class SoundExample : Layer
    {
        private SoundFile _song;

        public static void Main()
        {
            Debugger.TypeFilter.Add(MessageType.Trace);

            Context context = Starter.GetEmotionContext();

            context.LayerManager.Add(new LoadingScreen(), "loading", -1);
            context.LayerManager.Add(new SoundExample(), "Sound Example", 0);

            context.Start();
        }

        public override void Load()
        {
            _song = Context.AssetLoader.Get<SoundFile>("ElectricSleepMainMenu.wav");
            SoundLayer layer = Context.SoundManager.CreateLayer("example");
            Source source = Context.SoundManager.PlayOnLayer("example", _song);
            source.Looping = true;
            source.OnFinished += (e, a) => { Debugger.Log(MessageType.Info, MessageSource.Game, "Sound is over."); };

            SoundFadeIn effectTest = new SoundFadeIn(5000, Context.SoundManager.GetLayer("example"));
            layer.ApplySoundEffect(effectTest);
        }

        public override void Update(float frameTime)
        {
            if (Context.Input.IsKeyDown("A")) Context.SoundManager.GetLayer("example").Paused = !Context.SoundManager.GetLayer("example").Paused;

            if (Context.Input.IsKeyDown("W"))
            {
                Context.Settings.Volume += 10;
                Debugger.Log(MessageType.Info, MessageSource.Game, "Volume is at: " + Context.Settings.Volume);
            }

            if (Context.Input.IsKeyDown("S"))
            {
                Context.Settings.Volume -= 10;
                Debugger.Log(MessageType.Info, MessageSource.Game, "Volume is at: " + Context.Settings.Volume);
            }
        }

        public override void Draw(Renderer renderer)
        {
        }

        public override void Unload()
        {
        }
    }
}