using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading;
using Android.Util;

namespace BasicGLSurface
{
	[Activity (Label = "BasicGLSurface", MainLauncher = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTask)]
	public class BasicGLSurfaceActivity : Activity
	{
		private BasicGLSurfaceView m_View;
		const string TAG = "BasicGLSurfaceActivity";
		
		protected override void OnCreate (Bundle bundle)
		{
			Log.Info(TAG, "OnCreate");
			base.OnCreate (bundle);
		
			m_View = new BasicGLSurfaceView(Application);
			Log.Info(TAG, "Created BasicGLSurfaceView");
			
			// Set our view from the BasicGLSurfaceView
			SetContentView (m_View);
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


