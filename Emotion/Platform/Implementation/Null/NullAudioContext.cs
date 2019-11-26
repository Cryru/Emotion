#region Using

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Emotion.Audio;
using Emotion.IO;
using Emotion.Platform.Config;

#endregion

namespace Emotion.Platform.Implementation.Null
{
    public sealed class NullAudioContext : AudioContext
    {
        private List<AudioLayer> _layers = new List<AudioLayer>();

        public override string[] GetLayers()
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

        public override AudioLayer CreateLayer(string layerName, float layerVolume = 1)
        {
            var newLayer = new NullAudioLayer(layerName) {Volume = layerVolume};
            lock (_layers)
            {
                _layers.Add(newLayer);
            }

            return newLayer;
        }

        public override void RemoveLayer(string layerName)
        {
            AudioLayer layer = GetLayer(layerName);
            if(layer == null) return;

            layer.Stop();
            layer.Dispose();
            lock (_layers)
            {
                _layers.Remove(layer);
            }
        }

        public override AudioLayer GetLayer(string layerName)
        {
            lock (_layers)
            {
                return _layers.FirstOrDefault(layer => layer.Name == layerName);
            }
        }
    }
}