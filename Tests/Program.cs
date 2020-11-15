#region Using

using System;
using System.Collections.Generic;
using System.IO;
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
            {"compat", c => { c.RendererCompatMode = true; }},
            {
                "tag=ResizeTest", c =>
                {
                    c.RenderSize = new Vector2(320, 180);
                    c.UseIntermediaryBuffer = true;
                }
            },
            {"tag=EmotionDesktop testOnly", null},
            {
                "tag=Assets", c =>
                {
                    // Cleanup for storage tests.
                    if (!Directory.Exists("Player")) return;
                    Directory.Delete("Player", true);
                    Directory.CreateDirectory("Player");
                }
            },
            // {"tag=Scripting", null}, Broken since .Net 5.0
            {"tag=Coroutine", null},
            {"tag=StandardAudio", null},
            {"tag=Audio", null},
            {"tag=StandardText", null},
            {"tag=AnimatedTexture", null},
            {"tag=XML testOnly", null}
        };


        private static void Main(string[] args)
        {
            ResultDb.LoadCache();
            var config = new Configurator
            {
                HostSize = new Vector2(640, 360),
                RenderSize = new Vector2(640, 360)
            };
            Runner.RunTests(config, args, _otherConfigs, ResultDb.CachedResults);
        }
    }
}