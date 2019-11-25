using Emotion.Audio;
using System;
using System.Collections.Generic;
using System.Text;

namespace Emotion.Platform.Implementation.Null
{
    public class NullAudioLayer : AudioLayer
    {
        public NullAudioLayer(string name) : base(name)
        {

        }

        protected override void InternalStatusChange(PlaybackStatus oldStatus, PlaybackStatus newStatus)
        {

        }

        public override void Dispose()
        {

        }
    }
}
