#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Test;
using Emotion.Testing;
using Emotion.Utility;
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
            {"tag=Scripting", null},
            {"tag=StandardAudio", null},
            {"tag=Audio", null},
            {"tag=StandardText", null},
            {"tag=AnimatedTexture", null},
            {"tag=UITests", null},
            {"EMOTION_TEST_LIBRARY", null}
        };

        private static void Main(string[] args)
        {
            var config = new Configurator
            {
                HostSize = new Vector2(640, 360),
                RenderSize = new Vector2(640, 360),
                NoErrorPopup = true,
                UseEmotionFontSize = true,
                AudioQuality = AudioResampleQuality.HighHann,
                ExtraArgs = new[] {"software"} // Enable software renderer to ensure consistent results.
            };

            //Runner.RunAsRunner("EMOTION_TEST_LIBRARY", ref args);
            if (CommandLineParser.FindArgument(args, "EMOTION_TEST_LIBRARY", out string _))
            {
                TestExecutor.ExecuteTests(args, config);
                return;
            }

            //FontAsset.GlyphRasterizer = GlyphRasterizer.StbTrueType;
            ResultDb.LoadCache();
            Runner.RunTests(config, args, _otherConfigs, ResultDb.CachedResults);
        }
    }
}