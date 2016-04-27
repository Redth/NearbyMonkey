using Foundation;
using UIKit;
using System;
using Google.Nearby;

namespace NearbyMonkey.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		public const string NearbyApiKey = "AIzaSyCw5JuQY0z7oAyPlPPN4VJs4BfBSjNac34";

		public static IPublication PublishedMessage { get; set; }

		// class-level declarations
		UINavigationController navVC;

		public override UIWindow Window {
			get;
			set;
		}

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			// Override point for customization after application launch.
			// If not required for your application you can safely delete this method
			Window = new UIWindow (UIScreen.MainScreen.Bounds);
			navVC = new UINavigationController ();
			navVC.PushViewController (new MainViewController (), true);
			Window.RootViewController = navVC;
			Window.MakeKeyAndVisible ();

			return true;
		}

		public override void WillTerminate (UIApplication application)
		{
			// Before the app Terminate, delete your message from Nearby
			PublishedMessage.Dispose ();
			PublishedMessage = null;
		}
	}
}


