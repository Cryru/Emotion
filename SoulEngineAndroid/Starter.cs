using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using SoulEngine;
using SoulEngine.Modules;
using System.Diagnostics;

namespace SoulEngineAndroid
{
    [Activity(Label = Settings.WName
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
        , ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class Starter : Microsoft.Xna.Framework.AndroidGameActivity
    {
        /// <summary>
        /// A stopwatch used to track boot performance.
        /// </summary>
        public static Stopwatch bootPerformance;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Start measuring boot performance.
            bootPerformance = Stopwatch.StartNew();

            // Generate the default settings file to be used in case an external one is missing.
            Settings.GenerateDefaultFile();

            // Check if an external settings file exists, and load it's data if it does.
            Settings.ReadExternalSettings("\\settings.soul");

            Context.Core = new Core();
            SetContentView((View)Context.Core.Services.GetService(typeof(View)));
            // Measure boot time.
            bootPerformance.Stop();
            if (Context.Core.isModuleLoaded<Logger>())
                Context.Core.Module<Logger>().Add("Engine loading completed in: " + bootPerformance.ElapsedMilliseconds + "ms");
            Context.Core.Run();


        }
    }
}

