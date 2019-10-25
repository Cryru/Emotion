#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Platform.Implementation
{
    public abstract class AudioContext
    {
        public abstract void PlayAudioTest(AudioAsset wav);
    }
}