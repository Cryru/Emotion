#region Using

using Android.Content.PM;
using Emotion.Common;
using System.Collections;

#endregion

namespace Emotion.Droid.ExecTest
{
    [Activity(Label = "Android Test", MainLauncher = true, HardwareAccelerated = true, ScreenOrientation = ScreenOrientation.Landscape, Immersive = true,
        ConfigurationChanges = ConfigChanges.Navigation | ConfigChanges.Orientation | ConfigChanges.LayoutDirection | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout |
                               ConfigChanges.ColorMode | ConfigChanges.Density | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.Mcc | ConfigChanges.Mnc |
                               ConfigChanges.Touchscreen | ConfigChanges.FontScale | ConfigChanges.FontWeightAdjustment | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode |
                               ConfigChanges.Locale
    )]
    public class MainActivity : EmotionActivity
    {
        public override void Main(Configurator config)
        {
            config.DebugMode = true;
            //config.GlDebugMode = true;
            Engine.Start(config, EntryPointAsync);
        }

        protected IEnumerator EntryPointAsync()
        {
            yield return Engine.SceneManager.SetScene(new TestScene());
        }
    }
}