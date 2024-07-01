#region Using

using Android.Content.Res;
using Android.Views;
using Emotion.Common;

#endregion

namespace Emotion.Droid
{
    public abstract class EmotionActivity : Activity
    {
        public AndroidHost? Host;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Window.RequestFeature(WindowFeatures.NoTitle);

            if (Android.OS.Build.VERSION.SdkInt > Android.OS.BuildVersionCodes.S)
                Window.SetDecorFitsSystemWindows(false);

            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            Window.DecorView.SystemUiFlags = SystemUiFlags.LayoutFullscreen | SystemUiFlags.LayoutStable |
                SystemUiFlags.HideNavigation | SystemUiFlags.LayoutHideNavigation;

            // On the Android the application entry point (creation of the main activity)
            // is on an arbitrary UI thread, but we need to initialize the engine on the GL thread.
            // Therefore we pass a callback to the host which will be executed on the GL surface creation.
            Host = new AndroidHost(this, Main);
        }

        public abstract void Main(Configurator config);

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            Engine.Log.Error("Configuration changed!", "Android");
        }

        protected override void OnPause()
        {
            base.OnPause();
            Host?.AndroidContext.Surface.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            Host?.AndroidContext.Surface.OnResume();
        }
    }
}