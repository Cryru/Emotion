#region Using

using Emotion.Audio;

#endregion

namespace Emotion.Platform.Implementation.Null
{
    public class NullAudioLayer : AudioLayer
    {
        public NullAudioLayer(string name) : base(name)
        {
        }

        protected override void UpdateBackend()
        {
        }
    }
}