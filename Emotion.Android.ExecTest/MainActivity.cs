#region Using

using Android.Content.PM;
using Android.Content.Res;
using Emotion.Common;
using Emotion.ExecTest;

#endregion

namespace Emotion.Android.ExecTest
{
	[Activity(Label = "Android Test", MainLauncher = true, HardwareAccelerated = true,
		ConfigurationChanges = ConfigChanges.Navigation | ConfigChanges.Orientation | ConfigChanges.LayoutDirection | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout |
		                       ConfigChanges.ColorMode | ConfigChanges.Density | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.Mcc | ConfigChanges.Mnc |
		                       ConfigChanges.Touchscreen | ConfigChanges.FontScale | ConfigChanges.FontWeightAdjustment | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode |
		                       ConfigChanges.Locale
	)]
	public class MainActivity : Activity
	{
		public AndroidHost? Host;

		protected override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// On the Android the application entry point (creation of the main activity)
			// is on an arbitrary UI thread, but we need to initialize the engine on the GL thread.
			// Therefore we pass a callback to the host which will be executed on the GL surface creation.
			Host = new AndroidHost(this, config =>
			{
				config.DebugMode = true;
				config.GlDebugMode = true;
				Engine.Setup(config);
				Engine.SceneManager.SetScene(new Program());
				Engine.Run();
			});
		}

		public override void OnConfigurationChanged(Configuration newConfig)
		{
			base.OnConfigurationChanged(newConfig);
			Engine.Log.Warning("Configuration changed!", "Android");
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