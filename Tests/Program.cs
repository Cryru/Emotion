#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Test;
using Tests.Results;

#endregion

namespace Tests
{
    internal class Program
    {
        private static Dictionary<string, Action<Configurator>> _otherConfigs = new Dictionary<string, Action<Configurator>>
        {
            {"compat", c => c.SetRenderSettings(true)},
            {"tag=FullScale", c => c.SetRenderSize(new Vector2(320, 180))},
            //{"marginScale", c => c.SetRenderSize(null, false, false)},
            //{"marginScaleInteger", c => c.SetRenderSize(null, true, false)},
            {"tag=EmotionDesktop testOnly", null },
            {"tag=Assets", null },
            {"tag=Scripting", null },
            {"tag=Coroutine", null },
            {"tag=StandardAudio", null },
            {"tag=Audio", null },
            {"tag=StandardText", null },
            {"tag=AnimatedTexture", null }
        };


        private static void Main(string[] args)
        {
            ResultDb.LoadCache();
            Runner.RunTests(
                new Configurator()
                .SetHostSettings(new Vector2(640, 360)) // The resolution is set like that because it is the resolution of the render references.
                .SetRenderSize(fullScale: false),
            args, _otherConfigs, ResultDb.CachedResults);
        }
    }
}