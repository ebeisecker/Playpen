using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace iOSGLEssentials
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		EAGLView glView;
		
		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			//{
			//	RootViewController = new iOSGLEssentialsViewController()
			//};
			var frame = UIScreen.MainScreen.ApplicationFrame;
			glView = new EAGLView(frame);
			
			window.AddSubview(glView);
			window.MakeKeyAndVisible ();
			
			return true;
		}
		
		public override void OnResignActivation (UIApplication application)
		{
			glView.StopAnimation();
		}
		
		public override void DidEnterBackground (UIApplication application)
		{
			glView.StopAnimation();
		}
		
		public override void WillEnterForeground (UIApplication application)
		{
			glView.StartAnimation();
		}
		
		public override void OnActivated (UIApplication application)
		{
			glView.StartAnimation();
		}
		
		public override void WillTerminate (UIApplication application)
		{
			glView.StopAnimation();
		}
	}
}

