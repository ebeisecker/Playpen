using System;
using MonoTouch.OpenGLES;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.ObjCRuntime;
using MonoTouch.CoreAnimation;
using System.Drawing;

namespace iOSGLEssentials
{
	[Register("EAGLView")]
	public class EAGLView : UIView
	{
		#region Members
		ES2Renderer m_renderer;
		
		EAGLContext m_context;
		
		
		bool displayLinkSupported;
		
		CADisplayLink displayLink;
		NSTimer animationTimer;
		#endregion
			
		#region Properties
		public bool Animating
		{
			get;
			set;
		}
		
		private int animationFrameInterval;
		public int AnimationFrameInterval
		{
			get 
			{ 
				return animationFrameInterval; 
			}
			set 
			{
				// Frame interval defines how many display frames must pass between each time the
				// display link fires. The display link will only fire 30 times a second when the
				// frame interval is two on a display that refreshes 60 times a second. The default
				// frame interval setting of one will fire 60 tiemmes a second when the dispaly refreshes
				// at 60 times a second. A frame interval setting of less than one results in undefined
				// behavior
				if(value >= 1)
				{
					animationFrameInterval = value;
					
					if(Animating)
					{
						StopAnimation();
						StartAnimation();
					}
				}
			}
		}
		#endregion
		[Export("initWithCoder:")]
		public EAGLView (NSCoder coder)
			:base(coder)
		{
			Initialize();
		}
		
		public EAGLView(RectangleF frame)
			: base(frame)
		{
			Initialize();
		}
		
		public void Initialize()
		{ 
			CAEAGLLayer eaglLayer = (CAEAGLLayer)Layer;
			
			eaglLayer.Opaque = true;
			eaglLayer.DrawableProperties = NSDictionary.FromObjectsAndKeys(
				new object[] { NSNumber.FromBoolean(false), EAGLColorFormat.RGBA8 },
				new object[] { EAGLDrawableProperty.RetainedBacking, EAGLDrawableProperty.ColorFormat });
			
			m_context = new EAGLContext(EAGLRenderingAPI.OpenGLES2);
			
			if(m_context == null || !EAGLContext.SetCurrentContext(m_context))
			{
				throw new ApplicationException("Could not create/set EAGLContext");
			}
			
			m_renderer = new ES2Renderer();
			m_renderer.InitWithContext(m_context, (CAEAGLLayer)Layer);
			
			Animating = false;
			displayLinkSupported = false;
			animationFrameInterval = 1;
			displayLink = null;
			animationTimer = null;
			
			// A system version of 3.1 or greater is required to use CADisplayLink. The NSTimer
			// class is used as fallback when it isn't available.
			var reqSysVer = new NSString("3.1");
			var currSysVer = new NSString(UIDevice.CurrentDevice.SystemVersion);
			
			if(currSysVer.Compare(reqSysVer, NSStringCompareOptions.NumericSearch) != NSComparisonResult.Ascending)
				displayLinkSupported = true;
		}
	
		[Export("layerClass")]
		public static Class LayerClass()
		{
			return new Class(typeof(CAEAGLLayer));
		}
		
		public void DrawView()
		{
			EAGLContext.SetCurrentContext(m_context);
			m_renderer.Render();
		}
		
		public override void LayoutSubviews()
		{
			m_renderer.ResizeFromLayer((CAEAGLLayer)this.Layer);
			DrawView();
		}
		
		#region Public Methods
		public void StartAnimation()
		{
			if(!Animating)
			{
				if(displayLinkSupported)
				{
					// CADisplayLink is API new to iPhone SDK 3.1. Compiling against earlier versions will result in a warning, but can
					// be dismissed. If the system version runtime check for CADisplayLink exists in the Constructor. The runtime ensure 
					// this code will not be called in system versions earlier than 3.1.
					
					displayLink = CADisplayLink.Create(DrawView);
					displayLink.FrameInterval = AnimationFrameInterval;
					displayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoop.NSDefaultRunLoopMode);
				}
				else
				{
					animationTimer = NSTimer.CreateScheduledTimer((1.0 / 60.0) * AnimationFrameInterval, DrawView);
				}
				
				Animating = true;
			}
		}
		
		public void StopAnimation()
		{
			if(!Animating)
			{
				if(displayLinkSupported)
				{
					displayLink.Invalidate();
					displayLink = null;
				}
				else
				{
					animationTimer.Invalidate();
					animationTimer = null;
				}
				
				Animating = false;
			}
		}
		
		public new void Dispose ()
		{
			m_renderer.Dispose();
			
			// tear down context
			if(EAGLContext.CurrentContext == m_context)
				EAGLContext.SetCurrentContext(null);
			
			m_context.Dispose();
			
			base.Dispose();
		}
		#endregion
		
		
	}
}

