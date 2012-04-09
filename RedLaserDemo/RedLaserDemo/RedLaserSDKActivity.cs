using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using Com.Ebay.Redlasersdk;
using Com.Ebay.Redlasersdk.Scanner;

namespace RedLaserDemo
{
	[Activity (Label = "RedLaserSDKActivity")]
	public class RedLaserSDKActivity : Com.Ebay.Redlasersdk.Scanner.BarcodeScanActivity
	{
		const string TAG = "RedLaserSDK";
		
		public const string DO_UPCE = "do_upce";
		public const string DO_EAN8 = "do_ean8";
  		public const string DO_EAN13 = "do_ean13";
  		public const string DO_STICKY = "do_sticky";
  		public const string DO_QRCODE = "do_qrcode";
  		public const string DO_CODE128 = "do_code128";
  		public const string DO_CODE39 = "do_code39";
  		public const string DO_CODE93 = "do_code93";
  		public const string DO_DATAMATRIX = "do_datamatrix";
  		public const string DO_RSS14 = "do_rss14";
  		public const string DO_ITF = "do_itf";
		
		protected override int BeepResource
		{
			get { return R.Raw.Beep; }
		}
		
		protected override int LogoResource
		{
			get { return R.Drawable.OverlayLogo; }
		} 
		
		protected override void OnCreate (Bundle bundle)
		{
			Log.Debug(TAG, "Creating a new RedLaserSDK object");
			
			var settings = new RedLaserSettings();
			settings.AlignBarcode = "Align barcode inside box";
			settings.HoldStill = "Hold still for scan";
			
			base.OnCreate (bundle, settings);
			
			var extras = Intent.Extras.GetBundle("ScanBundle");
			
			Hints.Upce = extras.GetBoolean(DO_UPCE);
			Hints.Ean8 = extras.GetBoolean(DO_EAN8);
			Hints.Ean13 = extras.GetBoolean(DO_EAN13);
			Hints.QRCode = extras.GetBoolean(DO_QRCODE);
			Hints.Code128 = extras.GetBoolean(DO_CODE128);
			Hints.Code39 = extras.GetBoolean(DO_CODE39);
			Hints.Code93 = extras.GetBoolean(DO_CODE93);
			Hints.DataMatrix = extras.GetBoolean(DO_DATAMATRIX);
			Hints.ITF = extras.GetBoolean(DO_ITF);
			Hints.RSS14 = extras.GetBoolean(DO_RSS14);
			Hints.Sticky = extras.GetBoolean(DO_STICKY);
			
			//SetButtons("Button", "Button 2", "Button 3");
			
			Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
		}
		
		protected override void OnResume ()
		{
			base.OnResume ();
		}
		
		protected override sealed void OnButton1Click()
		{
			//throw new NotImplementedException ();
		}
		
		protected override sealed void OnButton2Click()
		{
			//throw new NotImplementedException ();
		}
		
		protected override sealed void OnButton3Click ()
		{
			//throw new NotImplementedException ();
		}
		
		protected void OnBarcodeScanned (BarcodeResult barcode)
		{
			ReturnResult(barcode);
		}
	}
}


