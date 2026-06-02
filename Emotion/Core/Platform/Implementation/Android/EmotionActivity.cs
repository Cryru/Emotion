#nullable enable

#region Using

using Android.Content.Res;
using Android.OS;
using Android.Views;
using Activity = Android.App.Activity;

#endregion

namespace Emotion.Core.Platform.Implementation.Android;

[DontSerialize]
public abstract class EmotionActivity : Activity
{
    public static EmotionActivity? MainActivity;
    public AndroidHost? Host;
    private bool _restartScheduled;

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

    #region Restart

    public bool IsRestartScheduled()
    {
        return _restartScheduled;
    }

    public void RestartApplication(string reason)
    {
        if (_restartScheduled) return;
        _restartScheduled = true;

        RunOnUiThread(() => RestartApplicationOnUiThread(reason));
    }

    private void RestartApplicationOnUiThread(string reason)
    {
        global::Android.Util.Log.Warn("Emotion", $"Restarting application: {reason}");

        // Super hacky way - we cant just restart the activity as the Engine global is mutated.
        // todo: Should we start handling losing graphics context???
        global::Android.Content.Intent? launchIntent = PackageManager?.GetLaunchIntentForPackage(PackageName);
        if (launchIntent != null)
        {
            launchIntent.AddFlags(global::Android.Content.ActivityFlags.ClearTop |
                                  global::Android.Content.ActivityFlags.ClearTask |
                                  global::Android.Content.ActivityFlags.NewTask);

            global::Android.App.PendingIntentFlags flags = global::Android.App.PendingIntentFlags.CancelCurrent;
            if (OperatingSystem.IsAndroidVersionAtLeast(23))
                flags |= global::Android.App.PendingIntentFlags.Immutable;

            global::Android.App.PendingIntent? restartIntent = global::Android.App.PendingIntent.GetActivity(this, 0, launchIntent, flags);
            var alarm = GetSystemService(AlarmService) as global::Android.App.AlarmManager;
            alarm?.Set(global::Android.App.AlarmType.Rtc, Java.Lang.JavaSystem.CurrentTimeMillis() + 200, restartIntent);
        }

        FinishAffinity();
        global::Android.OS.Process.KillProcess(global::Android.OS.Process.MyPid());
        Java.Lang.JavaSystem.Exit(0);
    }

    #endregion
}
