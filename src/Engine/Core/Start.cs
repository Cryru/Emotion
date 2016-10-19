//////////////////////////////////////////////////////////////////////////////
// Soul Engine - A game engine based on the MonoGame Framework.             //
//                                                                          //
// Copyright © 2016 Vlad Abadzhiev, MonoGame                                //
//                                                                          //
// The 'Start.cs" class is the file that controls the startup.              //
//                                                                          //
// Refer to the documentation for any questions, or                         //
// to TheCryru@gmail.com                                                    //
//////////////////////////////////////////////////////////////////////////////
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
        static void Main()
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
			Settings.Name += " (OpenGL-Mac)";

			//Mac application initialization.
			NSApplication.Init ();

			//using (var p = new NSAutoreleasePool ()) 
			//{
			//NSApplication.SharedApplication.Delegate = new AppDelegate();
			//NSApplication.Main(args);
			//}
#endif
#if LINUX //Linux requires no additional initialization.
            //Add suffix to engine.
            Settings.Name += " (OpenGL-Lin)";
#endif
#if WINDOWS //On Windows we need to start the logging system.
                    Log.fr_linkedApp = Core.Name + " " + Core.Ver;
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

#if __UNIFIED__ //Mac applications require these functions to work with their initialization system.
//class AppDelegate : NSApplicationDelegate
//{
//	public override void FinishedLaunching (MonoMac.Foundation.NSObject notification)
//	{
//		AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs a) =>  {
//		if (a.Name.StartsWith("MonoMac")) 
//		{
//			return typeof(MonoMac.AppKit.AppKitFramework).Assembly;
//		}
//			return null;
//		};
//		Core.Setup();
//	}

//	public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
//	{
//		return true;
//	}
//}  
#endif

} //Namespace ender.


