using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using OpenTK.Graphics.ES20;
using MonoTouch.CoreGraphics;
using System.Runtime.InteropServices;
using System.Drawing;

namespace IOSPlaypen
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		IOSPlaypenViewController viewController;

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
			
			viewController = new IOSPlaypenViewController ();
			
			window.RootViewController = viewController;
			window.MakeKeyAndVisible ();
			LoadImage(NSBundle.MainBundle.PathForResource("clouds", "jpg"), false);
				
			return true;
		}
		
		public static void LoadImage(string filePathName, bool flipVertical)
		{
			var imageClass = UIImage.FromFile(filePathName);
			
			var cgImage = imageClass.CGImage;

			var Width = cgImage.Width;
			var Height = cgImage.Height;
			var RowByteSize = Width * 4;
			var Data =  new byte[cgImage.Height * cgImage.Width]; 
			var Format = PixelInternalFormat.Rgba;
			var Type = PixelType.UnsignedByte;
			
			IntPtr dataPtr = Marshal.AllocHGlobal (cgImage.Height * cgImage.Width);
			
			using(var context = new CGBitmapContext(dataPtr, Width, Height, 8, RowByteSize, cgImage.ColorSpace, CGImageAlphaInfo.NoneSkipLast))
			{
				context.SetBlendMode(CGBlendMode.Copy);
				
				if(flipVertical)
				{
					context.TranslateCTM(0.0f, (float)Height);
					context.ScaleCTM(1.0f, -1.0f);
				}
				
				context.DrawImage(new RectangleF(0f, 0f, Width, Height), cgImage);
			}
			
			if(dataPtr == IntPtr.Zero)
			{
				return;
			}
			else
			{
				Marshal.PtrToStructure(dataPtr, Data);
			}
		}
	}
}

