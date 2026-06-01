#region Using

using Emotion.Core;
using Emotion.Core.Systems.Audio;
using Emotion.Testing;
using System.Numerics;
using System.Threading.Tasks;

#endregion

namespace Tests;

public static class Program
{
    public static async Task Main(string[] args)
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
        await TestExecutor.TestApplicationMain(args, config);
    }
}