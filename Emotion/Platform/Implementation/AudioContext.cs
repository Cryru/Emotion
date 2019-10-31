#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Emotion.Audio;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Platform.Implementation
{
    public abstract class AudioContext
    {
        public abstract string[] GetLayers();
        public abstract AudioLayer CreateLayer(string layerName, float layerVolume = 1f);
        public abstract void RemoveLayer(string layerName);
        public abstract AudioLayer GetLayer(string layerName);
    }
}