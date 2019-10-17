#region Using

using System.Numerics;
using Emotion.IO;
using Emotion.Platform.Config;

#endregion

namespace Emotion.Platform.Implementation.Null
{
    public sealed class NullAudioContext : AudioContext
    {
        public override void PlayAudioTest(WaveSoundAsset wav)
        {
           
        }
    }
}