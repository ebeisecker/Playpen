
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
using Com.Ebay.Redlasersdk.Scanner;

namespace RedLaserDemo
{
	public class BarcodeScanLayout : FrameLayout
	{
		public SurfaceView PreviewSurface { get; private set; }
		public ViewfinderView ViewFinderView { get; private set; }
		public ImageView LogoView {get; private set; }
		
		public BarcodeScanLayout(Context context, string holdStill, string alignBarcode)
			: base(context)
		{
			var param = new FrameLayout.LayoutParams(-1,  -1, GravityFlags.Center);
			
			PreviewSurface = new SurfaceView(context);
			AddView(PreviewSurface, param);
			
			ViewFinderView = new ViewfinderView(context, holdStill, alignBarcode);
			AddView(ViewFinderView, param);
			
			LogoView = new ImageView(context);
			AddView(LogoView, param);
		}
	}
}
