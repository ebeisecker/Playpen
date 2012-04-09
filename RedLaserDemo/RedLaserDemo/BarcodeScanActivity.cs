
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
using Com.Ebay.Redlasersdk.Network;
using Android.Util;
using Android.Graphics;
using Com.Ebay.Redlasersdk;
using Com.Ebay.Redlasersdk.Scanner;
using Android.Media;
using System.IO;

namespace RedLaserDemo
{
	[Activity (Label = "BarcodeScanActivity")]
	public abstract class BarcodeScanActivity : Activity, ISurfaceHolderCallback
	{
		class BeepListener : Java.Lang.Object, MediaPlayer.IOnCompletionListener
		{
			#region IOnCompletionListener implementation
			public void OnCompletion (MediaPlayer mediaPlayer)
			{
				mediaPlayer.SeekTo(0);
			}
			#endregion
		}
		
		#region Members
		private const string TAG = "BarcodeScanActivity";
		private const float BEEP_VOLUME = 0.1f;
		public const int MSG_DECODE_SUCCEEDED = 7747648;
		public const int MSG_DECODE_FAILEd = 7747648;
		public const int MSG_AUTOFOCUS = 7747650;
		protected const int MSG_QUIT = 7747651;
		public const int MSG_DECODE = 7747652;
		public const int MSG_RESTART_SCAN = 7747653;
		
		private CaptureActivityHandler handler;
		private ViewfinderView viewfinderView;
		private MediaPlayer mediaPlayer;
		private bool hasSurface;
		private bool playBeep;
		private int mWidth;
		private int mHeight;
		private float mDensity;
		private readonly MediaPlayer.IOnCompletionListener beepListener;
		private ViewfinderButtonsView mButtons;
		private BarcodeScanLayout mBarcodeScanLayout;
		private RedLaserSettings settings;
		#endregion
		
		#region Properties
		ViewfinderView ViewFinderView
		{
			get { return this.viewfinderView; }
		}
		
		Handler Handler
		{
			get { return this.handler; }
		}
		
		public BarcodeTypeHints Hints { get; set; }
		
		protected abstract int LogoResource { get; }
		
		protected abstract int BeepResource { get; }
		#endregion
		
		public BarcodeScanActivity()
		{
			this.beepListener = new BeepListener();
			
			this.Hints = new BarcodeTypeHints();
		}
		
		protected virtual void SetButtons(string buttonOneText, string buttonTwoText, string buttonThreeText)
		{
			//this.mButtons = this.mBarcodeScanLayout
		}
		
		protected virtual void OnCreate (Bundle bundle, RedLaserSettings settings)
		{
			base.OnCreate (bundle);

			this.settings = settings;
			
			StatusManager.Initialize(this);
			
			Log.Debug(TAG, "Starting capture activity");
			
			var window = Window;
			window.AddFlags(WindowManagerFlags.KeepScreenOn);
			this.mBarcodeScanLayout = new BarcodeScanLayout(this, this.settings.HoldStill, this.settings.AlignBarcode);
			SetContentView(this.mBarcodeScanLayout);
			
			var metrics = new DisplayMetrics();
			WindowManager.DefaultDisplay.GetMetrics(metrics);
			
			this.mWidth = metrics.WidthPixels;
			this.mHeight = metrics.HeightPixels;
			this.mDensity = metrics.Density;
			
			var logo = new ImageView(this);
			logo.SetScaleType(ImageView.ScaleType.Matrix);
			logo.SetImageResource(LogoResource);
			var matrix = new Matrix();
			matrix.PostRotate(270.0f);
			
			if(mWidth < mHeight) {
				var temp = mWidth;
				mWidth = mHeight;
				mHeight = temp;
			}
			
			var offset = 0;
			if(mDensity == 1.0f) {
				offset = 100;
			}
			
			matrix.PostTranslate((float)(mWidth / 2.0d + mWidth / 8 * mDensity) + offset, (float)(mHeight / 2.80 + 160.0f * mDensity));
			logo.ImageMatrix = matrix;
			
			CameraManager.Init(Application);
			viewfinderView = mBarcodeScanLayout.ViewFinderView;
			handler = null;
			hasSurface = false;
		}
		
		protected override void OnResume ()
		{
			Log.Debug(TAG, "Resuming!!!!!");
			base.OnResume ();
			
			var surfaceView = mBarcodeScanLayout.PreviewSurface;
			var surfaceHolder = surfaceView.Holder;
			
			if(hasSurface) {
				InitCamera(surfaceHolder);
			} 
			else {
				surfaceHolder.AddCallback(this);
				surfaceHolder.SetType(SurfaceType.PushBuffers);
			}
			ResetStatusView();
			
			playBeep = true;
			if(BeepResource == 0) {
				playBeep = false;	
			}
			
			InitBeepSound();
		}
		
		protected override void OnPause ()
		{
			base.OnPause ();
			if(handler != null) {
				handler.QuitSynchronously();
				handler = null;
			}
			
			CameraManager.Get().CloseDriver();
		}
		
		public override void OnConfigurationChanged (Android.Content.Res.Configuration newConfig)
		{
			base.OnConfigurationChanged (newConfig);
		}
		
		public void SurfaceCreated(ISurfaceHolder holder)
		{
			if(!hasSurface) {
				hasSurface = true;
				InitCamera(holder);
			}
		}
		
		public void SurfaceDestroyed(ISurfaceHolder holder)
		{
			hasSurface = false;
		}
		
		public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
		{
			
		}
		
		void HandleInRange(bool inRange)
		{
			ViewFinderView.SetInRange(inRange);
		}
		
		private void DenyDecode(string messageTitle)
		{
			OnPause();
			mBarcodeScanLayout.ViewFinderView.Visibility = ViewStates.Invisible;
			
			RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
			
			new AlertDialog.Builder(this).SetTitle(messageTitle).SetMessage("To continue developing with the RedLaer SDK, purchase a developer account at redlaser.com").
			SetPositiveButton("OK", (dialog, whichButton) => { this.Finish(); }).SetCancelable(false).Show();
		}
		
		void InitCamera(ISurfaceHolder holder)
		{
			try{
				CameraManager.Get().OpenDriver(holder);
			}
			catch(Java.IO.IOException ioe) {
				Log.Wtf(TAG, ioe);
				
			}
		}	
		
		void InitBeepSound()
		{
			if(playBeep && mediaPlayer == null)
			{
				VolumeControlStream = Android.Media.Stream.Music;
				mediaPlayer = new MediaPlayer();
				mediaPlayer.SetAudioStreamType(Android.Media.Stream.Music);
				mediaPlayer.SetOnCompletionListener(beepListener);
				
				var file = Resources.OpenRawResourceFd(BeepResource);
				try{
					mediaPlayer.SetDataSource(file.FileDescriptor, file.StartOffset, file.Length);
					
					file.Close();
					mediaPlayer.SetVolume(0.1f, 0.1f);
					mediaPlayer.Prepare();
				}
				catch(Java.IO.IOException e)
				{
					mediaPlayer = null;
				}
			}
		}
		
		void PlayBeepSound()
		{
			if(playBeep && mediaPlayer != null)	{
				mediaPlayer.Start();
			}
		}
	
		void DisplayFrameworkBugMessageAndExit()
		{
			var builder = new AlertDialog.Builder(this);
			builder.SetTitle("RedLaserSample");
			builder.SetMessage("Sorry, the Android camera encountered a problem. You may need to restart the device.");
			
			builder.SetPositiveButton(17039370, (dialogInterface, i) => this.Finish());
			builder.Show();
		}
		
		void ResetStatusView()
		{
			ViewFinderView.Visibility = ViewStates.Visible;
		}
		
		void DrawViewFinder()
		{
			ViewFinderView.DrawViewfinder();
		}
		
		protected abstract void OnButton1Click();
		
		protected abstract void OnButton2Click();
		
		protected abstract void OnButton3Click();
		
		protected void OnBarcodeScanned(BarcodeResult result)
		{
			ReturnResult(result);	
		}
		
		public void HandleDecode(BarcodeResult rawResult, Bitmap barcode)
		{
			PlayBeepSound();
			
			if(StatusManager.ScannerStatus == StatusManager.RLScannerStatus.DisabledSdkMode)
			{
				DenyDecode("Unregistered SDK Limit Reached");
			}
			else if(StatusManager.ScannerStatus == StatusManager.RLScannerStatus.Error)
			{
				DenyDecode("Redlaser Error");
			}
			else
			{
				Log.Debug("BarcodeScanActivity", rawResult.BarcodeType + ": " + rawResult.BarcodeString);
				
				var results = new List<BarcodeResult>();
				results.Add(rawResult);
				StatusManager.LogScans(results);
				
				OnBarcodeScanned(rawResult);
			}
		}
		
		protected void ReturnResult(BarcodeResult barcode)
		{
			if(barcode != null) {
				var returnIntent = new Intent();
				returnIntent.SetAction(barcode.BarcodeString);
				returnIntent.PutExtra("barcode_type", barcode.BarcodeType);
				SetResult (Result.Ok, returnIntent);
			}
			
			Finish();
		}
		
		protected void RestartScan() 
		{
			Handler.SendEmptyMessage(7747653);
		}
		
		
	}
}