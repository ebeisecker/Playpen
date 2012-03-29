// Toggle this to disable vertext buffer objects
// (i.e. use client-side vertext array objects)
// This must be defined if using the GL3 Core Profile on the Mac
#define USE_VERTEX_BUFFER_OBJECTS

// Toggle This to disable the rendering the reflection
// and setup of the GLSL program, model and FBO used for
// the reflection.
#define RENDER_REFLECTION

using System;
using MonoTouch.Foundation;
using OpenTK.Graphics.ES20;
using System.Drawing;

namespace iOSGLEssentials
{	
	public class OpenGLRenderer : NSObject
	{	
		#region Members
		
		uint m_defaultFBOName;
		
		const int POS_ATTRIB_IDX = 1;
		const int NORMAL_ATTRIB_IDX = 2;
		const int TEXCOORD_ATTRIB_IDX = 3;
		#endregion
		
		public OpenGLRenderer ()
		{
		}
		
		#region GL Members
	    void BufferOffset(int i)
		{

		}
		
#if RENDER_REFLECTION
		DemoModel m_quadModel;
		All m_quadPrimType;
		All m_quadElementType;
		int m_quadNumElements;
		int m_reflectVAOName;
		int m_reflectTexName;
		int m_reflectFBOName;
		int m_reflectWidth;
		int m_reflectHeight;
		int m_reflectPrgName;
		int m_reflectModelViewUniformIdx;
		int m_reflectProjectionuniformIdx;
		int m_reflectNormalMatrixUniformIdx;
#endif
		int m_characterPrgName;
		int m_characterMvpUniformIdx;
		int m_characterVAOName;
		int m_characterTexName;
		DemoModel m_characterModel;
		All m_characterPrimType;
		All m_characterElementType;
		int m_characterNumElements;
		float m_characterAngle;
		
		int m_viewWidth;
		int m_viewHeight;
		
		bool m_useVBOs;
		#endregion
		
		#region Public Methods
		public void ResizeWithWidth(int width, int height)
		{
			GL.Viewport(0, 0, width, height);
			
			m_viewWidth = width;
			m_viewHeight = height;
		}
		
		public void Render()
		{
			// Set up the modelview and projection matricies
			var modelView = new float[16];
			var projection = new float[16];
			var mvp = new float[16];
			
#if RENDER_REFLECTION
			
			GL.BindFramebuffer(All.Framebuffer, m_reflectFBOName);
			
			GL.Clear((int)All.ColorBufferBit | (int)All.DepthBufferBit);
			GL.Viewport(0, 0, m_reflectWidth, m_reflectHeight);
			
			MatrixUtil.LoadPerspective(ref projection, 90f, (float)m_reflectWidth / (float)m_reflectHeight, 5.0f, 10000f);
		
			MatrixUtil.LoadIdentity(ref modelView);
			
			//Invert Y so that everything is rendered up-side-down
			// as it should with a reflection
			
			MatrixUtil.ScaleApply(ref modelView, 1f, -1f, 1f);
			MatrixUtil.TranslateApply(ref modelView, 0, 300f, -800f);
			MatrixUtil.RotateXApply(ref modelView, -90.0f);
			MatrixUtil.RotateApply (ref modelView, m_characterAngle, .7f, .3f, 1f);
			
			MatrixUtil.Multiply(ref mvp, projection, modelView);
			
			// Use the program that we previously created
			GL.UseProgram(m_characterPrgName);
			
			// Set the modelview projection matrix that we calculated above
			// in our vertex shader
			GL.UniformMatrix4(m_characterMvpUniformIdx, 1, false, mvp);
			
			// Bind our vertex array object
			//GL.Vertex
			
			// Bind the texture to be used
			GL.BindTexture(All.Texture2D, m_characterTexName);
			
			// Cull front faces now that everything is flipped
			// with our inverted reflection transformation matrix
			GL.CullFace(All.Front);
			
			// Draw our object
			if(m_useVBOs)
			{
				GL.DrawElements(All.Triangles, m_characterNumElements, m_characterElementType, new int[0]);
			}
			else{
				GL.DrawElements(All.Triangles, m_characterNumElements, m_characterElementType, m_characterModel.Elements);
			}
			
			// Bind our default FBO to render to the screen
			GL.BindFramebuffer(All.Framebuffer, m_defaultFBOName);
			
			GL.Viewport(0, 0, m_viewWidth, m_viewHeight);
#endif // Render_REFLECTION
			
			GL.Clear((int)All.ColorBufferBit | (int)All.DepthBufferBit);
			
			// Use the program for rendering our character
			GL.UseProgram(m_characterPrgName);
			
			// Calculate the projection matrix
			MatrixUtil.LoadPerspective(ref projection, 90f, (float)m_viewWidth / (float)m_viewHeight, 5.0f, 10000f);
			
			// Calculate the modelview matrix to render our character
			// at the proper position and rotation
			MatrixUtil.LoadTranslate(ref modelView, 0f, 150f, -450f);
			MatrixUtil.RotateXApply(ref modelView, -90f);
			MatrixUtil.RotateApply(ref modelView, m_characterAngle, .7F, .3F, 1F);
			
			// Multiply the modelview and projection matrix and set it in the shader
			MatrixUtil.Multiply(ref mvp, projection, modelView);
			
			// Have our shader use the modelview projection matrix
			// that we calculated above
			GL.UniformMatrix4(m_characterMvpUniformIdx, 1, false, mvp);
			
			// Bind the texture to be used
			GL.BindTexture(All.Texture2D, m_characterTexName);
			
			// Cull back faces now that we no longer render
			// with an inverted matrix
			GL.CullFace(All.Back);
			
			// Draw our character
			// Draw our object
			if(m_useVBOs)
			{
				GL.DrawElements(All.Triangles, m_characterNumElements, m_characterElementType, new int[0]);
			}
			else{
				GL.DrawElements(All.Triangles, m_characterNumElements, m_characterElementType, m_characterModel.Elements);
			}
			
#if RENDER_REFLECTION
			
			// Use our shader for reflections
			GL.UseProgram(m_reflectPrgName);
			
			MatrixUtil.LoadTranslate(ref modelView, 0f, -50f, -250f);
			
			// Multiple the modelview and projection matrix and set it in the shader
			MatrixUtil.Multiply(ref mvp, projection, modelView);
			
			// Set the modelview matrix that we calculated above 
			// in our vertex shader
			GL.UniformMatrix4(m_reflectModelViewUniformIdx, 1, false, modelView);
			
			// Set the projection matrix that we calculted above
			// in our vertex shader
			GL.UniformMatrix4(m_reflectProjectionuniformIdx, 1, false, mvp);
			
			var normalMatrix = new float[9];
			
			// Calculate the normal matrix so that we can 
			// generate texture coordinates in our fragment shader
			
			// The normal matrix needs to be the inverse transpose of the 
			//   top left 3x3 portion of the modelview matrix
			// We don't need to calculate the inverse transpose matrix
			//   here because this will always be an orthonormal matrix
			//   thus the the inverse tranpose is the same thing
			MatrixUtil.mtx3x3FromTopLeftOf4x4(ref normalMatrix, modelView);
			
			// Set the normal matrix for our shader to use
			GL.UniformMatrix3(m_reflectNormalMatrixUniformIdx, 1, false, normalMatrix);
			
			// Bind the texture we rendered-to above (i.e. the reflection texture)
			GL.BindTexture(All.Texture2D, m_reflectTexName);
			
#if !ESSENTIAL_GL_PRACTICES_IOS
			// Generate mipmaps from the rendered-to base level
			//   Mipmaps reduce shimmering pixels due to better filtering
			// This call is not accelarated on iOS 4 so do not use
			//   mipmaps here
			GL.GenerateMipmap(All.Texture2D);
#endif
			
			// Draw our reflection plane
			if(m_useVBOs)
			{
				GL.DrawElements(All.Triangles, m_quadNumElements, m_quadElementType, new int[0]);
			}
			else{
				GL.DrawElements(All.Triangles, m_quadNumElements, m_quadElementType, m_quadModel.Elements);
			}
#endif // RENDER_REFLECTION
			
			// Update the angle so our charater keeps spinning
			m_characterAngle++;
		}
		
		unsafe static int GetGLTypeSize(All type)
		{
			switch(type)
			{
			case All.Byte:
				return sizeof(byte);
			case All.UnsignedByte:
				return sizeof(byte);
			case All.UnsignedShort:
				return sizeof(short);
			case All.Int:
				return sizeof(int);
			case All.UnsignedInt:
				return sizeof(uint);
			case All.Float:
				return sizeof(float);
			}
			
			return 0;
		}
		
		int BuildVAO(DemoModel model)
		{
			int vaoName = 0;
			
			// Create a vertex array object (VAO) to cache model parameters
			
			return vaoName;
		}
		
		public void Dispose()
		{
		}
		#endregion
	}
}

