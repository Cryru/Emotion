#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Editor.EditorHelpers;
using Emotion.ExecTest.TestGame;
using Emotion.Game.Time.Routines;
using Emotion.Game.World2D;
using Emotion.Game.World2D.SceneControl;
using Emotion.Game.World3D;
using Emotion.Game.World3D.SceneControl;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.IO;
using Emotion.Network.Base;
using Emotion.Network.BasicMessageBroker;
using Emotion.Network.ClientSide;
using Emotion.Network.ServerSide;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.Standard.Audio;
using Emotion.Standard.Audio.WAV;
using Emotion.Standard.Reflector;
using Emotion.Standard.XML;
using Emotion.Testing;
using Emotion.UI;
using Emotion.Utility;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.One.Work;
using WinApi.User32;

#endregion

namespace Emotion.ExecTest;

public class Program
{
    private static void Main(string[] args)
    {
        if (CommandLineParser.FindArgument(args, "tests", out string _))
        {
            MainTests(args);
            return;
        }

        Engine.Start(new Configurator
        {
            DebugMode = true,
            HostTitle = "Example"
        }, EntryPointAsync);
    }

    private static void MainTests(string[] args)
    {
        var config = new Configurator
        {
            DebugMode = true
        };

        TestExecutor.ExecuteTests(args, config);
    }

    private void ReflectorTest()
    {
        var data = ReflectorEngine.GetTypeHandler(typeof(Program));
        var members = data.GetMembers();
        foreach (var member in members)
        {
            member.ReadValueFromComplexObject(new Program(), out object? val);
        }
    }

    private static IEnumerator EntryPointAsync()
    {
        //yield return Engine.SceneManager.SetScene(new TestScene());
        yield return Engine.SceneManager.SetScene(new TestGame.TestScene());
    }

    private static void ResampleTest()
    {
        AudioConverter.SetResamplerQuality(AudioResampleQuality.ONE_ExperimentalOptimized);

        var targetFormat = new AudioFormat(32, true, 2, 48_000);

        float[] output = new float[1024 * 1024 * 35];
        int currentSample = 0;
        Span<float> currentOutput = output;

        var audio = Engine.AssetLoader.Get<AudioAsset>("Test/resample-test/MusicalPhrase_Mono_44.1.wav");

        while (true)
        {
            int framesGotten = audio.AudioConverter.GetResampledFrames(targetFormat, currentSample, 5000, currentOutput);
            if (framesGotten == 0) break;

            currentOutput = currentOutput.Slice(framesGotten * targetFormat.Channels);
            currentSample += framesGotten * targetFormat.Channels;
        }

        Span<float> result = new Span<float>(output, 0, currentSample * targetFormat.SampleSize);
        Span<byte> resultAsBytes = MemoryMarshal.Cast<float, byte>(result);
        byte[] wavFile = WavFormat.Encode(resultAsBytes, targetFormat);

        var fileName = AssetLoader.GetFileName(audio.Name);
        Engine.AssetLoader.Save(wavFile, $"Player/resample-test/{fileName}");
    }
}