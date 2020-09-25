#region Using

using System.Collections.Generic;
using System.Linq;
using Emotion.Audio;

#endregion

namespace Emotion.Platform.Implementation.Null
{
    public sealed class NullAudioContext : AudioContext
    {
        protected override AudioLayer CreateLayerInternal(string layerName)
        {
            return new NullAudioLayer(layerName);
        }
    }
}