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
            {"fullScale", c => c.SetRenderSize()},
            {"marginScale", c => c.SetRenderSize(null, false, false)},
            {"marginScaleInteger", c => c.SetRenderSize(null, true, false)}
        };

        private static string[] _otherRunners =
        {
            "conf=compat",
            "tag=EmotionDesktop testOnly",
            "tag=Assets",
            "tag=Scripting",
            "tag=Coroutine",
            "tag=FullScale conf=fullScale",
            "tag=StandardAudio",
            "tag=Audio"
        };

        private static void Main(string[] args)
        {
            ResultDb.LoadCache();
            Runner.RunTests(new Configurator().SetHostSettings(new Vector2(640, 360)).SetRenderSize(fullScale: false), args, _otherRunners, _otherConfigs, ResultDb.CachedResults);
        }
    }
}