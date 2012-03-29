using System;
using MonoTouch.OpenGLES;
using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using OpenTK.Graphics.ES20;

namespace iOSGLEssentials
{
	public class ES2Renderer : OpenGLRenderer
	{
		#region Members
		
		#endregion
		
		public ES2Renderer (EAGLContext context, CAEAGLLayer layer)
		{
			//GL.GenFramebuffers(1, 
		}
		
		#region Public Methods
		public void Render()
		{
		}
		
		public bool ResizeFromLayer(CAEAGLLayer layer)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}

