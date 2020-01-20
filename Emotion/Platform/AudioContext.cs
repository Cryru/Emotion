#region Using

using Emotion.Audio;

#endregion

namespace Emotion.Platform
{
    public abstract class AudioContext
    {
        public abstract string[] GetLayers();
        public abstract AudioLayer CreateLayer(string layerName, float layerVolume = 1f);
        public abstract void RemoveLayer(string layerName);
        public abstract AudioLayer GetLayer(string layerName);

        public virtual void Dispose()
        {
            string[] layers = GetLayers();
            foreach (string l in layers)
            {
                RemoveLayer(l);
            }
        }
    }
}