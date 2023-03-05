#region Using

using Emotion.Common;

#endregion

namespace Emotion.Android.ExecTest
{
	[Activity(Label = "@string/app_name", MainLauncher = true, HardwareAccelerated = true)]
	public class MainActivity : Activity
	{
		protected override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// On the Android the application entry point (creation of the main activity)
			// is on an arbitrary UI thread, but we need to initialize the engine on the GL thread.
			// Therefore we pass a callback to the host which will be executed on the GL surface creation.
			new AndroidHost(this, config =>
			{
				config.HostTitle = "Android Test";
				config.DebugMode = true;
				config.GlDebugMode = true;
				Engine.Setup(config);
				Engine.SceneManager.SetScene(new Emotion.ExecTest.Program());
				Engine.Run();
			});
		}
	}
}