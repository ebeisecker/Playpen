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

namespace BasicGLSurface
{
	class BasicGLSurfaceView : GLSurfaceView
	{
		public BasicGLSurfaceView(Context context)
			:base(context)
		{
			SetEGLContextClientVersion(2);
			SetRenderer(new GLES20TriangleRenderer(context));
		}
	}
}

