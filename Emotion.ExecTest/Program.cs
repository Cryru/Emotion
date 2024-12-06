#region Using

using System.Runtime.InteropServices;
using Emotion.Audio;
using Emotion.ExecTest.ExamplesOne;
using Emotion.IO;
using Emotion.Standard.Audio;
using Emotion.Standard.Audio.WAV;
using Emotion.Standard.Reflector;
using Emotion.Utility;

#endregion

namespace Emotion.ExecTest;

public class Program
{
    public static void Main(string[] args)
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
        //yield return Engine.SceneManager.SetScene(new Example3D());
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