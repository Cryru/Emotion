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
            Context context = Starter.GetEmotionContext();

            context.LayerManager.Add(new LoadingScreen(), "loading", -1);
            context.LayerManager.Add(new SoundExample(), "Sound Example", 0);

            context.Start();
        }

        public override void Load()
        {
            _song = Context.AssetLoader.Get<SoundFile>("ElectricSleepMainMenu.wav");
            Source source = Context.SoundManager.PlaySoundLayer("example", _song, false, true);
            source.OnFinished += (e, a) => { Debugger.Log(MessageType.Info, MessageSource.Game, "Sound is over."); };

            SoundFadeIn effectTest = new SoundFadeIn(5000, source);
            Context.SoundManager.AddEffect(effectTest);

            source.Play();
        }

        public override void Update(float frameTime)
        {
            if (Context.Input.IsKeyDown("A"))
                if (Context.SoundManager.GetSoundLayer("example").isPlaying)
                    Context.SoundManager.PauseSoundLayer("example");
                else
                    Context.SoundManager.ResumeSoundLayer("example");

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