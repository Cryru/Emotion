//////////////////////////////////////////////////////////////////////////////
// SoulEngine - A game engine based on the MonoGame Framework.              //
//                                                                          //
// Copyright © 2016 Vlad Abadzhiev - TheCryru@gmail.com                     //
//                                                                          //
// For any questions and issues: https://github.com/Cryru/SoulEngine        //
//////////////////////////////////////////////////////////////////////////////
/// <summary>
/// The engine's boot code for several platforms is here.
/// </summary>
using System;
using System.Threading;
using Microsoft.Xna.Framework;
#if WINDOWS //Windows uses a Soul library to log errors. This is not available on other platforms.
using Soul;
#endif
#if __UNIFIED__
using AppKit;
using Foundation;
#endif
#if ANDROID //Android needs these references to run.
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Content.PM;
using Android.Views;
using Android.Widget;
using Android.OS;
#endif

namespace SoulEngine
{
#if ANDROID //The Android starting method is different from other platforms.

	[Activity(Label = "Soul Engine (Android)",
			   MainLauncher = true,
			   Icon = "@drawable/icon",
			   Theme = "@style/Theme.Splash",
			   AlwaysRetainTaskState = true,
			   LaunchMode = LaunchMode.SingleInstance,
			   ConfigurationChanges = ConfigChanges.Orientation |
									  ConfigChanges.KeyboardHidden |
									  ConfigChanges.Keyboard |
									  ConfigChanges.ScreenSize,
               ScreenOrientation = ScreenOrientation.Landscape)]

	public class Activity1 : AndroidGameActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
            //Pass the activity context to the core.
            Core.androidHost = this;
			//Setup the host. We setup the host outside of the core for Android because the ContentView requires an Activity class.
			Core.host = new Engine();
			SetContentView(Core.host.Services.GetService<View>());

            //Add the affix.
            Core.Name += " (Android)";
			//Setup the Engine Core.
			Core.Setup();
		}
	}
#endif

#if !ANDROID //Skip the normal starting class on Android.
    public static class Program
    {
#if WINDOWS //On Windows we use a mutex GUID system to prevent multiple instances.
        static Mutex mutex = new Mutex(true, "{" + Core.GUID + "}");
#endif
#if !__UNIFIED__ //On Linux and Windows we need to specify single threadedness.
        [STAThread]
#endif
        static void Main(string[] args)
        {
#if WINDOWS //The extension of the Windows' multi instance mutex system.
            //Check for multi instancing.
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                //Start up.
                try
                {
                    //Start the logging system.
                    Log.enabled = Settings.debugLogging;
                    Log.NewLog();
                    Log.maxLog = 5;
                    //Add suffix to engine.
                    Core.Name += " (OpenGL-Win)";
#endif
#if __UNIFIED__ //On Mac we need to initialize the application through this.
			//Add suffix to engine.
			Core.Name += " (OpenGL-Mac)";

			//Mac application initialization.
			NSApplication.Init ();
#endif
#if LINUX //Linux requires no additional initialization.
            //Add suffix to engine.
            Settings.Name += " (OpenGL-Lin)";
#endif
#if WINDOWS //On Windows we need to start the logging system.
                    Log.fr_linkedApp = Core.Name + " " + Core.Version;
#endif
                    //Run the engine.
                    Core.Setup();
#if WINDOWS //The tail end of the logging system on Windows.
                }
                catch (Exception e) //Error handling.
                {
                    //Write the current message to the log.
                    Log.Add(e.ToString());
                    //Dump the log.
                    Log.DumpLog();
                    throw;
                }
            }
#endif
        }
    } //Program class ender.
#endif

} //Namespace ender.


