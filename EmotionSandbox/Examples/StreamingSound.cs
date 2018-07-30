// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.GLES;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Sound;

#endregion

namespace EmotionSandbox.Examples
{
    public class StreamingSound : Layer
    {
        public static void Main()
        {
            Debugger.TypeFilter.Add(MessageType.Trace);

            Context context = Starter.GetEmotionContext();

            context.LayerManager.Add(new LoadingScreen(), "loading", -1);
            context.LayerManager.Add(new StreamingSound(), "Sound Stream Example", 0);

            context.Start();
        }

        public override void Load()
        {
            SoundFile[] files = {Context.AssetLoader.Get<SoundFile>("1.wav"), Context.AssetLoader.Get<SoundFile>("2.wav")};
            SoundLayer layer = Context.SoundManager.CreateLayer("example");
            StreamingSource source = Context.SoundManager.StreamOnLayer("example", files);
            source.Looping = true;
            source.OnFinished += (e, a) => { Debugger.Log(MessageType.Info, MessageSource.Game, "Sound is over."); };
        }

        private int _prev = -1;

        public override void Update(float frameTime)
        {
            int id = ((StreamingSource) Context.SoundManager.GetLayerSource("example")).FileId;

            if (_prev == id) return;
            Debugger.Log(MessageType.Info, MessageSource.Game, "Streaming source is playing id " + id + " which is " + Context.SoundManager.GetLayerSource("example").FileName);
            _prev = id;
        }

        public override void Draw(Renderer renderer)
        {
        }

        public override void Unload()
        {
        }
    }
}