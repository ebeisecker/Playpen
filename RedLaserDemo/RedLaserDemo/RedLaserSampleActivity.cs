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
using Android.Util;

//[assembly:UsesLibrary("com.ebay.redlasersdk")]
using Com.Ebay.Redlasersdk.Scanner;
                      
namespace RedLaserDemo
{	
	[Activity (Name="monodroid.samples.redlasersample", Label = "RedLaserSampleActivity", MainLauncher = true)]			
	public class RedLaserSampleActivity : Activity
	{
		const string TAG = "RedLaserSampleActivity";
		
		bool doUpce = true;
		bool doEan8 = false;
		bool doEan13 = false;
		bool doSticky = false;
		bool doQRCode = false;
		bool doCode128 = false;
		bool doCode39 = false;
		bool doCode93 = false;
		bool doDataMatrix = false;
		bool doRSS14 = false;
		bool doITF = false;
		
		private string udid;
		  		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			Title = "RedLaser SDK Demo";
			
			SetContentView(Resource.Layout.Main);
			
			this.udid = RedLaserSettings.GetDeviceID(ContentResolver);
			
			var btnScan = (Button)FindViewById(Resource.Id.btn_scan); // btnScan
			btnScan.Click += (sender, e) => this.LaunchScanner();
			
			var udidView = (TextView)FindViewById(Resource.Id.udid_string); // udidString
			udidView.SetText("UDID: " + this.udid, TextView.BufferType.Normal);
			
			var toggleUPC = (ToggleButton)FindViewById(Resource.Id.toggleUPC); // toggleUPC
			toggleUPC.Checked = this.doUpce;
			toggleUPC.Click += (sender, e) => 
			{
				if(toggleUPC.Checked){
					doUpce = true;
					Toast.MakeText(this, "OPC On", 0).Show();
				}
				else{
					doUpce = false;
					Toast.MakeText(this, "OPC off", 0).Show();
				}
			};
			
			var toggleEAN = (ToggleButton)FindViewById(Resource.Id.toggleEAN); // Toggle EAN
			toggleEAN.Checked = doEan8;
			toggleEAN.Click += (sender, e) => 
			{
				if(toggleEAN.Checked){
					doEan8 = true;
					doEan13 = true;
					Toast.MakeText(this, "EAN On", 0).Show();
				}
				else{
					doEan8 = false;
					doEan13 = false;
					Toast.MakeText(this, "EAN Off", 0).Show();
				}
			};
			
			var toggleQR = (ToggleButton)FindViewById(Resource.Id.toggleQR); // Toggle QR
			toggleQR.Checked = doQRCode;
			toggleQR.Click += (sender, e) => 
			{
				if(toggleQR.Checked){
					doQRCode = true;
					Toast.MakeText(this, "QR Code On", 0).Show();
				}
				else {
					doQRCode = false;
					Toast.MakeText(this, "QR Code Off", 0).Show();
				}
			};
			
			var toggle128 = (ToggleButton)FindViewById(Resource.Id.toggle128); // toggle128
			toggle128.Checked = doCode128;
			toggle128.Click += (sender, e) => 
			{
				if(toggle128.Checked){
					doCode128 = true;
					Toast.MakeText(this, "Code 128 On", 0).Show();
				}
				else{
					doCode128 = false;
					Toast.MakeText(this, "Code 128 Off", 0).Show();
				}
			};
			
			var toggle39 = (ToggleButton)FindViewById(Resource.Id.toggle39); // toggle39
			toggle39.Checked = doCode39;
			toggle39.Click += (sender, e) => 
			{
				if(toggle39.Checked){
					doCode39 = true;
					Toast.MakeText(this, "Code 39 On", 0).Show();
				}
				else{
					doCode39 = false;
					Toast.MakeText(this, "Code 39 Off", 0).Show();
				}
			};
		}
		
		protected override void OnResume ()
		{
			base.OnResume ();
			
			var toggleUPC = (ToggleButton)FindViewById(Resource.Id.toggleUPC); // toggleUPC
			toggleUPC.Checked = doUpce;
			
			var toggleEAN = (ToggleButton)FindViewById(Resource.Id.toggleEAN); // Toggle EAN
			toggleEAN.Checked = doEan8;
			
			var toggleQR = (ToggleButton)FindViewById(2131165187); // Toggle QR
			toggleQR.Checked = doQRCode;
			
			var toggle128 = (ToggleButton)FindViewById(2131165188); // toggle128
			toggle128.Checked = doCode128;
			
			var toggle39 = (ToggleButton)FindViewById(2131165189); // toggle39
			toggle39.Checked = doCode39;
		}
		
		protected override void OnActivityResult (int requestCode, Result resultCode, Android.Content.Intent data)
		{
			if(resultCode == Result.Ok){
				var barcode = data.Action;
				var barcodeType = data.GetStringExtra("barcode_type");
				Log.Debug(TAG, "BARCODE: " + barcode);
				
				new AlertDialog.Builder(this).SetTitle("Scan Result").SetMessage(barcodeType + ": " + barcode)
				.SetNegativeButton("OK", (Dialog, whichButton) => { }).Show();
			}
		}
		
		public override bool OnCreateOptionsMenu (IMenu menu)
		{
		    base.OnCreateOptionsMenu (menu);
			var inflater = MenuInflater; 
			inflater.Inflate(Resource.Menu.main, menu); // main
			return true;
		}
		
		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch(item.ItemId)
			{
			case Resource.Id.scan: // scan
				LaunchScanner();
				return true;
			}
			
			return base.OnOptionsItemSelected(item);
		}
		
		public void LaunchScanner()
		{
			try
			{
				var bundle = BuildBundle();
				var scanIntent = new Intent(this, typeof(RedLaserSDKActivity));
				scanIntent.PutExtra("ScanBundle", bundle);
				StartActivityForResult(scanIntent, 1);
			}
			catch(Exception e)
			{
				Log.Debug(TAG, e.Message + " " + e.StackTrace);
			}
		}
		
		public Bundle BuildBundle()
		{
			Bundle bundle = new Bundle();
			
			bundle.PutBoolean(RedLaserSDKActivity.DO_UPCE, doUpce);
			bundle.PutBoolean(RedLaserSDKActivity.DO_EAN8, doEan8);
			bundle.PutBoolean(RedLaserSDKActivity.DO_EAN13, doEan13);
			bundle.PutBoolean(RedLaserSDKActivity.DO_STICKY, doSticky);
			bundle.PutBoolean(RedLaserSDKActivity.DO_QRCODE, doQRCode);
			bundle.PutBoolean(RedLaserSDKActivity.DO_CODE128, doCode128);
			bundle.PutBoolean(RedLaserSDKActivity.DO_CODE39, doCode39);
			bundle.PutBoolean(RedLaserSDKActivity.DO_CODE93, doCode93);
			bundle.PutBoolean(RedLaserSDKActivity.DO_DATAMATRIX, doDataMatrix);
			bundle.PutBoolean(RedLaserSDKActivity.DO_RSS14, doRSS14);
			bundle.PutBoolean(RedLaserSDKActivity.DO_ITF, doITF);
			
			return bundle;
		}
	}
}

