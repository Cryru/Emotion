#region Using

using System.Collections.Generic;
using System.Linq;
using Emotion.Common;
using Emotion.Platform;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Audio
{
    public class AudioContext
    {
        protected List<AudioLayer> _layers = new();
        protected IAudioAdapter _adapter;

        public AudioContext(IAudioAdapter adapter)
        {
            _adapter = adapter;
        }

        public AudioLayer CreateLayer(string layerName, float layerVolume = 1)
        {
            layerName = layerName.ToLower();
            AudioLayer newLayer = _adapter.CreatePlatformAudioLayer(layerName);
            newLayer.Volume = layerVolume;
            lock (_layers)
            {
                _layers.Add(newLayer);
            }

            Engine.Log.Info($"Created audio layer {newLayer.Name}", MessageSource.Audio);
            return newLayer;
        }

        public void RemoveLayer(string layerName)
        {
            layerName = layerName.ToLower();
            AudioLayer layer = GetLayer(layerName);
            if (layer == null) return;

            layer.Stop();
            layer.Dispose();
            lock (_layers)
            {
                _layers.Remove(layer);
            }

            Engine.Log.Info($"Removed audio layer {layer.Name}", MessageSource.Audio);
        }

        public AudioLayer GetLayer(string layerName)
        {
            layerName = layerName.ToLower();
            lock (_layers)
            {
                return _layers.FirstOrDefault(layer => layer.Name == layerName);
            }
        }

        public string[] GetLayers()
        {
            string[] names;
            lock (_layers)
            {
                names = new string[_layers.Count];
                for (var i = 0; i < _layers.Count; i++)
                {
                    names[i] = _layers[i].Name;
                }
            }

            return names;
        }

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