#region Using

using System;
using Emotion.Common;

#endregion

namespace Emotion.Web
{
    public class EmotionSetupService
    {
        public Action<Configurator> SetupEngine;
    }
}