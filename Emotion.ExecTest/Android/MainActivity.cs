#region Using

using Android.App;
using Android.Content.PM;
using Emotion.ExecTest.ExamplesOne;
using Emotion.Platform.Implementation.Android;

#endregion

namespace Emotion.ExecTest.Android;

[Activity(
    // App name
    Label = "Emotion Demo",
    
    // These are the recommended properties for all Emotion activities:
    MainLauncher = true, HardwareAccelerated = true, ScreenOrientation = ScreenOrientation.Landscape, Immersive = true,
    ConfigurationChanges = ConfigChanges.Navigation | ConfigChanges.Orientation | ConfigChanges.LayoutDirection | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout |
                           ConfigChanges.ColorMode | ConfigChanges.Density | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.Mcc | ConfigChanges.Mnc |
                           ConfigChanges.Touchscreen | ConfigChanges.FontScale | ConfigChanges.FontWeightAdjustment | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode |
                           ConfigChanges.Locale
)]
public class MainActivity : EmotionActivity
{
    public override void Main()
    {
        Program.Main(Array.Empty<string>());
    }
}