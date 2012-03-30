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
		EAGLContext m_context;
		
		int m_colorRenderbuffer;
		int m_depthRenderbuffer;
		#endregion
		
		public ES2Renderer ()
		{
		}
		
		public void InitWithContext (EAGLContext context, CAEAGLLayer drawable)
		{
			GL.GenFramebuffers(1, out m_defaultFBOName);
			
			// Create default framebuffer object. The backing will be allocated for the current layer in resizeFromLayer
			GL.GenRenderbuffers(1, out m_colorRenderbuffer);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, m_defaultFBOName);
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, m_colorRenderbuffer);
			m_context = context;
			
			// This call associates the storage for the current render buffer with the EAGLDrawable (our CAEGLLAyer)
			// allowing us to draw into a buffer that will later be rendered to the screen wherever the layer is
			//(which correspondes with our) view.
			m_context.RenderBufferStorage((uint)All.Renderbuffer, drawable);
			
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, RenderbufferTarget.Renderbuffer, m_colorRenderbuffer);
			
			int backingWidth;
			int backingHeight;
			GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferWidth, out backingWidth);
			GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferHeight, out backingHeight);
			
			GL.GenRenderbuffers(1, out m_depthRenderbuffer);
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, m_depthRenderbuffer);
			GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferInternalFormat.DepthComponent16, backingWidth, backingHeight);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.DepthAttachment, RenderbufferTarget.Renderbuffer, m_depthRenderbuffer);
			
			if(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
			{
				//Console.WriteLine(");
				throw new ApplicationException("Failed to make complete framebuffer object: " + GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));
			}
			
			InitWithDefaultFBO(m_defaultFBOName);
		}
		
		#region Public Methods
		public override void Render()
		{
			// Replace the implementation of this method to do your own custom drawing
			
			
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, m_defaultFBOName);
			
			base.Render();
			
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, m_colorRenderbuffer);
			m_context.PresentRenderBuffer((uint)All.Renderbuffer);
		}
		
		public bool ResizeFromLayer(CAEAGLLayer layer)
		{
			// The pixel dimensions of the CAEAGLLayer
			int backingWidth;
			int backingHeight;
			
			// Allocate color buffer backing based on the current layer size
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, m_colorRenderbuffer);
			m_context.RenderBufferStorage((uint)RenderbufferTarget.Renderbuffer, layer);
			GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferWidth, out backingWidth);
			GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferHeight, out backingHeight);
			
			GL.GenRenderbuffers(1, out m_depthRenderbuffer);
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, m_depthRenderbuffer);
			GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferInternalFormat.DepthComponent16, backingWidth, backingHeight);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.DepthAttachment, RenderbufferTarget.Renderbuffer, m_depthRenderbuffer);
			
			base.ResizeWithWidth(backingWidth, backingHeight);
			
			if(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
			{
				Console.WriteLine("Failed to make complete framebuffer object", GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));
				return false;
			}
			
			return true;
		}
		
		public new void Dispose ()
		{
			if(m_defaultFBOName != 0)
			{
				GL.DeleteFramebuffers(1, ref m_defaultFBOName);
				m_defaultFBOName = 0;
			}
			
			if(m_colorRenderbuffer != 0)
			{
				GL.DeleteRenderbuffers(1, ref m_colorRenderbuffer);
				m_colorRenderbuffer = 0;
			}
			
			base.Dispose();
		}
		#endregion
	}
}

