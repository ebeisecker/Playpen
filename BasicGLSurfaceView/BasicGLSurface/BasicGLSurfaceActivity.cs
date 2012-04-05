using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading;

namespace BasicGLSurface
{
	[Activity (Label = "BasicGLSurfaceView", MainLauncher = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTask)]
	public class BasicGLSurfaceViewActivity : Activity
	{
		private BasicGLSurfaceView m_View;
		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			
			m_View = new BasicGLSurfaceView(Application);
			
			// Set our view from the BasicGLSurfaceView
			SetContentView (m_View);
			Thread.Sleep(10000);
		}
		
		protected override void OnPause ()
		{
			base.OnPause ();
			m_View.OnPause();
		}
		
		protected override void OnResume ()
		{
			base.OnResume ();
			m_View.OnResume();
		}
	}
}


