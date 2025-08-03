#nullable enable

#region Using

using Android.Content.Res;
using Android.OS;
using Android.Views;
using Emotion.Core.Platform.Implementation.Android;
using Activity = Android.App.Activity;

#endregion

namespace Emotion.Platform.Implementation.Android;

[DontSerialize]
public abstract class EmotionActivity : Activity
{
    public static EmotionActivity? MainActivity;
    public AndroidHost? Host;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        MainActivity = this;
        AssertNotNull(Window);

        base.OnCreate(savedInstanceState);
        Window.RequestFeature(WindowFeatures.NoTitle);
        Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
        //if (OperatingSystem.IsAndroidVersionAtLeast(30))
        //    Window.SetDecorFitsSystemWindows(false);

        if (OperatingSystem.IsAndroidVersionAtLeast(30))
        {
            IWindowInsetsController? insetController = Window?.DecorView?.WindowInsetsController;
            insetController?.Hide(WindowInsets.Type.SystemBars());
        }
        else
        {
            Window.DecorView.SystemUiFlags = SystemUiFlags.ImmersiveSticky |
                SystemUiFlags.LayoutFullscreen | SystemUiFlags.LayoutStable |
                SystemUiFlags.HideNavigation | SystemUiFlags.LayoutHideNavigation;
        }

        // On the Android the application entry point (creation of the main activity)
        // is on an arbitrary UI thread, but we need to initialize the engine on the GL thread.
        // Therefore we pass a callback to the host which will be executed on the GL surface creation.
        Host = new AndroidHost(this);
    }

    public abstract void Main();

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