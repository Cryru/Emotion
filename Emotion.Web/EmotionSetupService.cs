#region Using

using System;
using System.Threading.Tasks;
using Emotion.Common;

#endregion

namespace Emotion.Web
{
    public class EmotionSetupService
    {
        public Func<Configurator, Task> SetupEngine;
    }
}