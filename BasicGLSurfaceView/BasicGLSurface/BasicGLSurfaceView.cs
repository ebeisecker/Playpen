using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Opengl;
using Android.Util;

namespace BasicGLSurface
{
	class BasicGLSurfaceView : GLSurfaceView
	{
		const string TAG = "BasicGLSurfaceView";
		
		public BasicGLSurfaceView(Context context)
			:base(context)
		{
			SetEGLContextClientVersion(2);
			Log.Info(TAG, "SetClientVersion to 2");
			
			var render = new GLES20TriangleRenderer(context); 
			Log.Info(TAG, "Created GLES20TriangleRender");
			
			SetRenderer(render);		
			Log.Info(TAG, "SetRenderer");
		}
	}
}

