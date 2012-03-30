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
using System.Runtime.InteropServices;
using System.Text;

namespace iOSGLEssentials
{	
	public class OpenGLRenderer : NSObject
	{	
		#region Members
		
		protected int m_defaultFBOName;
		
		const int POS_ATTRIB_IDX = 1;
		const int NORMAL_ATTRIB_IDX = 2;
		const int TEXCOORD_ATTRIB_IDX = 3;
		#endregion
		
		#region GL Members
	    void BufferOffset(int i)
		{

		}
		
#if RENDER_REFLECTION
		protected DemoModel m_quadModel;
		protected All m_quadPrimType;
		protected DrawElementsType m_quadElementType;
		protected int m_quadNumElements;
		protected int m_reflectVAOName;
		protected int m_reflectTexName;
		protected int m_reflectFBOName;
		protected int m_reflectWidth;
		protected int m_reflectHeight;
		protected int m_reflectPrgName;
		protected int m_reflectModelViewUniformIdx;
		protected int m_reflectProjectionuniformIdx;
		protected int m_reflectNormalMatrixUniformIdx;
#endif
		protected int m_characterPrgName;
		protected int m_characterMvpUniformIdx;
		protected int m_characterVAOName;
		protected int m_characterTexName;
		protected DemoModel m_characterModel;
		protected All m_characterPrimType;
		protected DrawElementsType m_characterElementType;
		protected int m_characterNumElements;
		protected float m_characterAngle;
		
		protected int m_viewWidth;
		protected int m_viewHeight;
		
		protected bool m_useVBOs;
		#endregion
		
		public OpenGLRenderer ()
		{
		}
		
		protected void InitWithDefaultFBO(int defaultFBOName)
		{
			Console.WriteLine(string.Format("{0} {1}", GL.GetString(StringName.Renderer), GL.GetString(StringName.Version)));
			
			////////////////////////////////////////////////////
			// Build all of our and setup initial state here  //
			// Don't wait until our real time run loop begins //
			////////////////////////////////////////////////////
			m_defaultFBOName = defaultFBOName;
			
			m_viewWidth = 100;
			m_viewHeight = 100;
			
			m_characterAngle = 0;
#if USE_VERTEX_BUFFER_OBJECTS
			m_useVBOs = true;
#else
			m_useVBOs = false;
#endif
			
			var filePathName = string.Empty;
			
			//////////////////////////////
			// Load our character model //
			//////////////////////////////
			
			filePathName = NSBundle.MainBundle.PathForResource("GLData/demon", "model");
			m_characterModel = DemoModel.LoadModel(filePathName);
			
			// Build Vertex uffer Objects (VBOs) and Vertex Array Objects (VAOs) with our model data
			m_characterVAOName = BuildVAO(m_characterModel);
			
			// Cache the number of element and primtType to use later in our GL.DrawElements calls
			m_characterNumElements = m_characterModel.NumElements;
			m_characterPrimType = m_characterModel.PrimType;
			m_characterElementType = m_characterModel.ElementType;
			
			if(m_useVBOs)
			{
				//If we're using VBOs we can destroy all this memory since buffers are
				// loaded into GL and we've saved anything else we need
				m_characterModel = null;
			}
			
			
			////////////////////////////////////
			// Load texture for our character //
			////////////////////////////////////
			
			filePathName = NSBundle.MainBundle.PathForResource("GLData/demon", "png");
			var image = DemoImage.LoadImage(filePathName, false);
			
			// Build a texture object with our image data
			m_characterTexName = BuildTexture(image);
			
			////////////////////////////////////////////////////
			// Load and Setup shaders for character rendering //
			////////////////////////////////////////////////////

			DemoSource vtxSource = null;
			DemoSource frgSource = null;
			
			filePathName = NSBundle.MainBundle.PathForResource("Shaders/character", "vsh");
			vtxSource = DemoSource.LoadSource(filePathName);
			
			filePathName = NSBundle.MainBundle.PathForResource("Shaders/character", "fsh");
			frgSource = DemoSource.LoadSource(filePathName);
			
			// Build program
			m_characterPrgName = BuildProgramWithVertexSource(vtxSource, frgSource, false, true);
			
			m_characterMvpUniformIdx = GL.GetUniformLocation(m_characterPrgName, "modelViewProjectionMatrix");
			
			if(m_characterMvpUniformIdx < 0)
				Console.WriteLine("No modelViewProjectionMatrix in character shader");
			
#if RENDER_REFLECTION
			
			m_reflectWidth = 512;
			m_reflectHeight = 512;
			
			////////////////////////////////////////////////
			// Load a model for a quad for the reflection //
			////////////////////////////////////////////////
			
			m_quadModel = DemoModel.LoadQuadModel();
			
			// build Vertex Buffer Objects (VBOs) and Vertex Array Object (VAOs) with our model data
			m_reflectVAOName = BuildVAO(m_quadModel);
			
			// Cache the number of element and prim type to use later in our GL.DrawElements calls
			m_quadNumElements = m_quadModel.NumElements;
			m_quadPrimType = m_quadModel.PrimType;
			m_quadElementType = m_quadModel.ElementType;
			
			if(m_useVBOs)
			{
				// Release quad Model;
				m_quadModel = null;
			}
			
			/////////////////////////////////////////////////////
			// Create texture and FBO for reflection rendering //
			/////////////////////////////////////////////////////
			
			m_reflectFBOName = BuildFBOWithWidth(m_reflectWidth, m_reflectHeight);
			
			// Get the texture we created in buildReflectFBO by binding the
			// reflection FBO and getting the buffer attached to color 0
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, m_reflectFBOName);
			
			int iReflectTexName = 0;
			
			GL.GetFramebufferAttachmentParameter(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, FramebufferParameterName.FramebufferAttachmentObjectName, out iReflectTexName);
			
			m_reflectTexName = iReflectTexName;
			
			/////////////////////////////////////////////////////
			// Load and setup shaders for reflection rendering //
			/////////////////////////////////////////////////////
			
			filePathName = NSBundle.MainBundle.PathForResource("Shaders/reflect", "vsh");
			vtxSource = DemoSource.LoadSource(filePathName);
			
			filePathName = NSBundle.MainBundle.PathForResource("Shaders/reflect", "fsh");
			frgSource = DemoSource.LoadSource (filePathName);
			
			// Build Program
			m_reflectPrgName = BuildProgramWithVertexSource(vtxSource, frgSource, true, false);
			
			m_reflectModelViewUniformIdx = GL.GetUniformLocation(m_reflectPrgName, "modelViewMatrix");
			
			if(m_reflectModelViewUniformIdx < 0)
				Console.WriteLine("No modelViewMatrix in reflection shader");
			
			m_reflectProjectionuniformIdx = GL.GetUniformLocation(m_reflectPrgName, "modelViewProjectionMatrix");
			
			if(m_reflectProjectionuniformIdx < 0)
				Console.WriteLine("No modelViewProjectionMatrix in reflection shader");
			
			m_reflectNormalMatrixUniformIdx = GL.GetUniformLocation(m_reflectPrgName, "normalMatrix");
			
			if(m_reflectNormalMatrixUniformIdx <0)
				Console.WriteLine("No normalMatrix in reflection shader");

#endif // RENDER_REFLECTION
			
			////////////////////////////////////////////////
			// Set up OpenGL state that will never change //
			////////////////////////////////////////////////
			
			// Depth test will always be enabled
			GL.Enable(EnableCap.DepthTest);
			
			// We will always cull back faces for better performance
			GL.Enable(EnableCap.CullFace);
			
			// Always use this clear color
			GL.ClearColor(.5f, .4f, .5f, 1.0f);
			
			// Draw our scene once without presenting the rendered image.
			// This is done in order to pre-warm OpenGL
			// We don't need to present the buffer since we don't actually want the
			// user to see this, we're only drawing as a pre-warm stage
			Render();
			
			// Reset the m_characterAngle which is incremented in render
			m_characterAngle = 0;
			
			// Check for errors to make sure all of our setup went ok
			GetGLError();
		}
		
		#region Public Methods
		public void ResizeWithWidth(int width, int height)
		{
			GL.Viewport(0, 0, width, height);
			
			m_viewWidth = width;
			m_viewHeight = height;
		}
		
		public virtual void Render()
		{
			// Set up the modelview and projection matricies
			var modelView = new float[16];
			var projection = new float[16];
			var mvp = new float[16];
			
#if RENDER_REFLECTION
			
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, m_reflectFBOName);
			
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
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
			GL.Oes.BindVertexArray(m_characterVAOName);
			
			// Bind the texture to be used
			GL.BindTexture(TextureTarget.Texture2D, m_characterTexName);
			
			// Cull front faces now that everything is flipped
			// with our inverted reflection transformation matrix
			GL.CullFace(CullFaceMode.Front);
			
			// Draw our object
			if(m_useVBOs)
			{
				GL.DrawElements(BeginMode.Triangles, m_characterNumElements, m_characterElementType, new int[0]);
			}
			else{
				GL.DrawElements(BeginMode.Triangles, m_characterNumElements, m_characterElementType, m_characterModel.Elements);
			}
			
			// Bind our default FBO to render to the screen
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, m_defaultFBOName);
			
			GL.Viewport(0, 0, m_viewWidth, m_viewHeight);
#endif // Render_REFLECTION
			
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			
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
			GL.BindTexture(TextureTarget.Texture2D, m_characterTexName);
			
			// Bind our vertex array object
			GL.Oes.BindVertexArray(m_characterVAOName);
			
			// Cull back faces now that we no longer render
			// with an inverted matrix
			GL.CullFace(CullFaceMode.Back);
			
			// Draw our character
			// Draw our object
			if(m_useVBOs)
			{
				GL.DrawElements(BeginMode.Triangles, m_characterNumElements, m_characterElementType, new int[0]);
			}
			else{
				GL.DrawElements(BeginMode.Triangles, m_characterNumElements, m_characterElementType, m_characterModel.Elements);
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
			GL.BindTexture(TextureTarget.Texture2D, m_reflectTexName);
			
#if !ESSENTIAL_GL_PRACTICES_IOS
			// Generate mipmaps from the rendered-to base level
			//   Mipmaps reduce shimmering pixels due to better filtering
			// This call is not accelarated on iOS 4 so do not use
			//   mipmaps here
			GL.GenerateMipmap(TextureTarget.Texture2D);
#endif
			// Bind our vertex array object
			GL.Oes.BindVertexArray(m_reflectVAOName);
			
			// Draw our reflection plane
			if(m_useVBOs)
			{
				GL.DrawElements(BeginMode.Triangles, m_quadNumElements, m_quadElementType, new int[0]);
			}
			else{
				GL.DrawElements(BeginMode.Triangles, m_quadNumElements, m_quadElementType, m_quadModel.Elements);
			}
#endif // RENDER_REFLECTION
			
			// Update the angle so our charater keeps spinning
			m_characterAngle++;
		}
		
		unsafe static int GetGLTypeSize(VertexAttribPointerType type)
		{
			
			switch(type)
			{
			case VertexAttribPointerType.UnsignedByte:
				return sizeof(byte);
			case VertexAttribPointerType.UnsignedShort:	
				return sizeof(short);
			case VertexAttribPointerType.Byte:
				return sizeof(byte);
			case VertexAttribPointerType.Float:
				return sizeof(float);
			//case VertexAttribPointerType.Fixed:
				
			}
			/*switch(type)
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
			}*/
			
			return 0;
		}
		
		unsafe int BuildVAO(DemoModel model)
		{
			int vaoName = 0;
			
			// Create a vertex array object (VAO) to cache model parameters
			GL.Oes.GenVertexArrays(1, out vaoName);
			GL.Oes.BindVertexArray(vaoName);
	
			if(m_useVBOs)
			{
				int posBufferName = 0;
				
				// Creat a vertex buffer object (VBO) to store positions
				GL.GenBuffers(1, out posBufferName);
				
				// Allocate and load position data into the VBO
				//int size = (int) model.PositionArraySize;
				//fixed (void *ptr = &size) {
				//	GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr (ptr), model.Positions, BufferUsage.StaticDraw);
				//}
				GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)model.PositionArraySize, model.Positions, BufferUsage.StaticDraw);
				
				// Enable the position attribute for this VAO
				GL.EnableVertexAttribArray(POS_ATTRIB_IDX);
				
				// Get the size of the position type so we can set the stride properly
				var posTypeSize = GetGLTypeSize(model.PositionType);
				
				// Setup parameters for position attribute in the VAO including,
				// size, type, stride, and offset in the currently bound VAO
				// This also attaches the position VBO to the VAO
				GL.VertexAttribPointer(POS_ATTRIB_IDX, 		// What attribute index will this array feed in the vertex shader (see BuildProgram)
				                       model.PositionSize,	// How many element are there per position?
				                       model.PositionType,  // What is the type of this data?
				                       false,				// Do we want to normalize this data (0-1 range for fixed-point types)
				                       model.PositionSize * posTypeSize, // What is the stride (i.e. bytes between position)?
				                       new int[0]);
				
				if(model.Normals != null)
				{
					int normalBufferName = 0;
					
					// Create a vertex buffer object (VBO) to store positions
					GL.GenBuffers(1, out normalBufferName);
					GL.BindBuffer(BufferTarget.ArrayBuffer, normalBufferName);
					
					// Allocate and load normal data into the VBO
					GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)model.NormalArraySize, model.Normals, BufferUsage.StaticDraw);
					
					// Enable the normal attribute for this VAO
					GL.EnableVertexAttribArray(NORMAL_ATTRIB_IDX);
					
					// Get the size of the normal type so we can set the stride properly
					var normalTypeSize = GetGLTypeSize(model.NormalType);
					
					// Set up parameters for position attribute in the VAO including,
					// size, type, stride, and offset in the currently bound VAO
					// This also attaches the position VBO to the VAO
					GL.VertexAttribPointer(NORMAL_ATTRIB_IDX, // What attribue index will this array feed in the vertex shader
					                       model.NormalSize,  // How many elements are there per normal?
					                       model.NormalType,  // What is the type of this data?
					                       false,			  // Do we want to normalize this data (0-1 range for fixed-point types)
					                       model.NormalSize * normalTypeSize, // What is the stride.
					                       new int[0]);
				}
				
				if(model.TexCoords != null)
				{
					int texcoordBufferName = 0;
					
					// Create a VBO to store texcoords
					GL.GenBuffers(1, out texcoordBufferName);
					GL.BindBuffer(BufferTarget.ArrayBuffer, texcoordBufferName);
					
					// Allocate and load texcoord data into the VBO
					GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)model.TexCoordArraySize, model.TexCoords, BufferUsage.StaticDraw);
					
					// Enable the texcoord attribute for this VAO
					GL.EnableVertexAttribArray(TEXCOORD_ATTRIB_IDX);
					
					// Get the size of the texcoord type so we can set the stride properly
					var texcoordTypeSize = GetGLTypeSize(model.TexCoordType);
					
					// Set up parameters for texcoord attribute in the VAO including,
					// size, type, stride, and oofset in the currently bound VAO
					// This also attaches the texcoord VBO to VAO
					GL.VertexAttribPointer(TEXCOORD_ATTRIB_IDX, // What attribute index will this array feed in the vertex shader
					                       model.TexCoordsSize, // How many elements are there per texture coord?
					                       model.TexCoordType,  // What is the type of this data in the array?
					                       true,				// Do we want to normalize this data
					                       model.TexCoordsSize * texcoordTypeSize, // What is the stride
					                       new int[0]);
				}
				
				int elementBufferName = 0;
				
				// Create a VBO to vertex array elements
				// This also attaches the element array buffer to the VAO
				GL.GenBuffers(1, out elementBufferName);
				GL.BindBuffer(BufferTarget.ArrayBuffer, elementBufferName);
				
				// Allocate and load vertex array element data into VBO
				GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)model.ElementArraySize, model.Elements, BufferUsage.StaticDraw);
			}
			else{
				
				// Enable the position attribute for this VAO
				GL.EnableVertexAttribArray(POS_ATTRIB_IDX);
				
				// Get the size of the position type so we can set the stride propertly
				var posTypeSize = GetGLTypeSize(model.PositionType);
				
				// Set up parameters for position attribute in the VAO including,
				// size, type, stride, and offset in the currently bound VAO
				// This also attaches the position VBO to the VAO
				GL.VertexAttribPointer(POS_ATTRIB_IDX,  // What attibute index will this array feed in the vertex shader? (also see buildProgram)
							 		 model.PositionSize,  // How many elements are there per position?
									 model.PositionType,  // What is the type of this data
							  		 false,				// Do we want to normalize this data (0-1 range for fixed-pont types)
							     	 model.PositionSize*posTypeSize, // What is the stride (i.e. bytes between positions)?
							  		 model.Positions);    // Where is the position data in memory?
				
				if(model.Normals != null)
				{			
					// Enable the normal attribute for this VAO
					GL.EnableVertexAttribArray(NORMAL_ATTRIB_IDX);
					
					// Get the size of the normal type so we can set the stride properly
					var normalTypeSize = GetGLTypeSize(model.NormalType);
					
					// Set up parmeters for position attribute in the VAO including, 
					//   size, type, stride, and offset in the currenly bound VAO
					// This also attaches the position VBO to the VAO
					GL.VertexAttribPointer(NORMAL_ATTRIB_IDX,	// What attibute index will this array feed in the vertex shader (see buildProgram)
										  model.NormalSize,	// How many elements are there per normal?
										  model.NormalType,	// What is the type of this data?
										  false,				// Do we want to normalize this data (0-1 range for fixed-pont types)
										  model.NormalSize * normalTypeSize, // What is the stride (i.e. bytes between normals)?
										  model.Normals);	    // Where is normal data in memory?
				}
				
				if(model.TexCoords != null)
				{
					// Enable the texcoord attribute for this VAO
					GL.EnableVertexAttribArray(TEXCOORD_ATTRIB_IDX);
					
					// Get the size of the texcoord type so we can set the stride properly
					var texcoordTypeSize = GetGLTypeSize(model.TexCoordType);
					
					// Set up parmeters for texcoord attribute in the VAO including, 
					//   size, type, stride, and offset in the currenly bound VAO
					// This also attaches the texcoord array in memory to the VAO	
					GL.VertexAttribPointer(TEXCOORD_ATTRIB_IDX,	// What attibute index will this array feed in the vertex shader (see buildProgram)
										  model.TexCoordsSize,	// How many elements are there per texture coord?
										  model.TexCoordType,	// What is the type of this data in the array?
										  false,				// Do we want to normalize this data (0-1 range for fixed-point types)
										  model.TexCoordsSize * texcoordTypeSize,  // What is the stride (i.e. bytes between texcoords)?
										  model.TexCoords);	// Where is the texcood data in memory?
				}

			}			
			
			GetGLError();
			
			return vaoName;
		}
		
		void DestroyVAO(int vaoName)
		{
				int index = 0;
				int bufName = -1;
				
				// Bind the VAO so we can get data from it
				GL.Oes.BindVertexArray(vaoName);
				
				// For every possible attribute set in the VAO
				for(index = 0; index < 16; index++)
				{
					// Get the VBO set for that attibute
					GL.GetVertexAttrib(index, VertexAttribParameter.VertexAttribArrayBufferBinding, out bufName);
					
					// If there was a VBO set...
					if(bufName != -1)
					{
						//...delete the VBO
						GL.DeleteBuffers(1, ref bufName);
					}
				}
				
				// Get any element array VBO set in the VAO
				GL.GetInteger(GetPName.ElementArrayBufferBinding, out bufName);
				
				// If there was a element array VBO set in the VAO
				if(bufName != -1)
				{
					//...delete the VBO
					GL.DeleteBuffers(1, ref bufName);
				}
				
				// Finally, delete the VAO
				GL.Oes.DeleteVertexArrays(1, ref vaoName);
				
				GetGLError();
		}
		
		int BuildTexture(DemoImage image)
		{
			int texName = 0; 
			
			// Create a texture object to apply to model
			GL.GenTextures(1, out texName);
			GL.BindTexture(TextureTarget.Texture2D, texName);
			
			// Set up filter and wrap modes for this texture object
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapLinear);
			
			// Indicate that pixel rows are tightly packed 
			//  (defaults to stride of 4 which is kind of only good for
			//  RGBA or FLOAT data types)
			GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
			
			// Allocate and load image data into texture
			GL.TexImage2D<byte>(TextureTarget.Texture2D, 0, image.Format, image.Width, image.Height, 0,
						 (PixelFormat)Enum.ToObject(typeof(PixelFormat), (int)image.Format), image.Type, image.Data);
			
			// Create mipmaps for this texture for better image quality
			GL.GenerateMipmap(TextureTarget.Texture2D);
			
			GetGLError();
			
			return texName;
		}
		
		void DeleteFBOAttachment(FramebufferSlot attachment)
		{    
		    int param = 0;
		    int objName = 0;
			
		    GL.GetFramebufferAttachmentParameter(FramebufferTarget.Framebuffer, attachment,
		                                          FramebufferParameterName.FramebufferAttachmentObjectType,
		                                          out param);
			
		    if((int)RenderbufferTarget.Renderbuffer == param)
		    {
		        GL.GetFramebufferAttachmentParameter(FramebufferTarget.Framebuffer, attachment,
		                                              FramebufferParameterName.FramebufferAttachmentObjectName,
		                                              out param);
				
		        objName = param;
		        GL.DeleteRenderbuffers(1, ref objName);
		    }
		    else if((int)All.Texture == param)
		    {
		        
		        GL.GetFramebufferAttachmentParameter(FramebufferTarget.Framebuffer, attachment,
		                                              FramebufferParameterName.FramebufferAttachmentObjectName,
		                                              out param);
				
		        objName = param;
		        GL.DeleteTextures(1, ref objName);
		    }
		    
		}		
		
		void DestroyFBO(int fboName)
		{ 
			if(0 == fboName)
			{
				return;
			}
		    
		    GL.BindFramebuffer(FramebufferTarget.Framebuffer, fboName);
			
			
		    int maxColorAttachments = 1;
			
			int colorAttachment = 0; 
			
			// For every color buffer attached
		    for(colorAttachment = 0; colorAttachment < maxColorAttachments; colorAttachment++)
		    {
				// Delete the attachment
				DeleteFBOAttachment(FramebufferSlot.ColorAttachment0);
			}
			
			// Delete any depth or stencil buffer attached
		    DeleteFBOAttachment(FramebufferSlot.DepthAttachment);
			
		    DeleteFBOAttachment(FramebufferSlot.StencilAttachment);
			
		    GL.DeleteFramebuffers(1, ref fboName);
		}
		
		int BuildFBOWithWidth(int width, int height)
		{
			int fboName = 0;
	
			int colorTexture = 0;
			
			// Create a texture object to apply to model
			GL.GenTextures(1, out colorTexture);
			GL.BindTexture(TextureTarget.Texture2D, colorTexture);
			
			// Set up filter and wrap modes for this texture object
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
		
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
			
			// Allocate a texture image with which we can render to
			// Pass NULL for the data parameter since we don't need to load image data.
			//     We will be generating the image by rendering to this texture
			byte[] dummy = null;
			GL.TexImage2D<byte>(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
						 PixelFormat.Rgba, PixelType.UnsignedByte, dummy);
			
			int depthRenderbuffer = 0;
			GL.GenRenderbuffers(1, out depthRenderbuffer);
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRenderbuffer);
			GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferInternalFormat.DepthComponent16, width, height);
			
			GL.GenFramebuffers(1, out fboName);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, fboName);	
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, colorTexture, 0);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.DepthAttachment, RenderbufferTarget.Renderbuffer, depthRenderbuffer);
			
			if(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
			{
				Console.WriteLine("failed to make complete framebuffer object %x", Enum.GetName (typeof(All), GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer)));
				DestroyFBO(fboName);
				return 0;
			}
			
			GetGLError();
			
			return fboName;
		}		
		
		int BuildProgramWithVertexSource(DemoSource vertexSource, DemoSource fragmentSource, bool hasNormal, bool hasTexcoord)
		{
			int prgName = 0;
			int logLength = 0, status = 0;
			
			// String to pass to Gl.ShaderSource
			var sourceString = string.Empty;
			
			// Determine if GLSL version 140 is supported by this context.
			// We'll use this info to generate a GLSL shader source string
			// with the proper version preprocessor string prepended
			float glLanguageVersion;
			
			glLanguageVersion = Convert.ToSingle(GL.GetString( StringName.ShadingLanguageVersion));
			
			//  All.ShadingLanguageVersion returns the version standard version form
			//  with decimals, but the GLSL version preprocessor directive simply
			//  uses integers (thus 1.10 should 110 and 1.40 should be 140, etc.)
			//  We multiply the floating point number by 100 to get a proper
			//  number for the GLSL preprocessor directive
			int version = Convert.ToInt32(100 * glLanguageVersion);
			
			prgName = GL.CreateProgram();
			
			// Indicate the attribute indicies on which vertex arrays will be
			// set with GL.VertexAttribPointer
			// See BuildVAO to see where vertex array are actually set
			GL.BindAttribLocation(prgName, POS_ATTRIB_IDX, "inPosition");
			
			if(hasNormal)
			{
				GL.BindAttribLocation(prgName, NORMAL_ATTRIB_IDX, "inNormal");
			}
			
			if(hasTexcoord)
			{
				GL.BindAttribLocation(prgName, TEXCOORD_ATTRIB_IDX, "inTexcoord");
			}
			
			//////////////////////////////////////
			// Specify and compile VertexShader //
			//////////////////////////////////////
			
			sourceString = string.Format("#version {0}\n{1}", version, vertexSource.String);
			
			int vertexShader;
			if(!CompileShader(out vertexShader, ShaderType.VertexShader, sourceString))
			{
				Console.WriteLine("Could not Compile Vertex Shader");
				return 0;
			}
			
			GL.AttachShader(prgName, vertexShader);
			
			/////////////////////////////////////////
			// Specify and compile Fragment Shader //
			/////////////////////////////////////////
			
			sourceString = string.Format("#version {0}\n{1}", version, fragmentSource.String);
			
			int fragShader;
			if(!CompileShader(out fragShader, ShaderType.FragmentShader, sourceString))
			{
				Console.WriteLine("Could not Compile Fragment Shader");
				return 0;
			}
			
			// Attach the fragment shader to our program
			GL.AttachShader(prgName, fragShader);
			
			//////////////////////
			// Link the program //
			//////////////////////
			
			GL.LinkProgram(prgName);
			GL.GetProgram(prgName,  ProgramParameter.InfoLogLength, out logLength);
			if(logLength > 0)
			{
				var log = new StringBuilder(logLength);
				GL.GetProgramInfoLog(prgName, logLength, out logLength, log);
				Console.WriteLine("Program link log: " + log.ToString());
			}
			
			GL.GetProgram(prgName, ProgramParameter.LinkStatus, out status);
			if(status == 0)
			{
				Console.WriteLine("Failed to link program");
				return 0;
			}
			
			GL.ValidateProgram(prgName);
			GL.GetProgram(prgName, ProgramParameter.InfoLogLength, out logLength);
			if(logLength > 0)
			{
				var log = new StringBuilder(logLength);
				GL.GetProgramInfoLog(prgName, logLength, out logLength, log);
				Console.WriteLine("Program validate log: " + log.ToString());
			}
			
			GL.GetProgram(prgName, ProgramParameter.ValidateStatus, out status);
			if(status == 0)
			{
				Console.WriteLine("Failed to validate program");
				return 0;
			}
			
			GL.UseProgram(prgName);
		
			///////////////////////////////////////
			// Setup common program input points //
			///////////////////////////////////////
			
			int sampleLoc = GL.GetUniformLocation(prgName, "diffuseTexture");
			
			// Indicate that the diffuse texture will be bound to texture uint 0
			var uint0 = 0;
			GL.Uniform1 (sampleLoc, uint0);
			
			GetGLError();
			
			return prgName;
		}
		
		bool CompileShader (out int shader, ShaderType type, string path)
		{
			string shaderProgram = System.IO.File.ReadAllText (path);
			int len = shaderProgram.Length, status = 0;
			shader = GL.CreateShader (type);

			GL.ShaderSource (shader, 1, new string [] { shaderProgram }, ref len);
			GL.CompileShader (shader);
			GL.GetShader (shader, ShaderParameter.CompileStatus, out status);
			
			if (status == 0) {
				GL.DeleteShader (shader);
				return false;
			}
			return true;
		}
		
		unsafe void DestroyProgram(int prgName)
		{
			if(0 == prgName)
				return;
			
			int shaderNum;
			int shaderCount = 0;
			
			// Get the number of attached shaders
			GL.GetProgram(prgName, ProgramParameter.AttachedShaders, out shaderCount);
			
			var shaders = new int[shaderCount * sizeof(int)];
			
			// Get the names of the shaders attached to the program
			GL.GetAttachedShaders(prgName, 
			                      shaderCount,
			                      new int[] { shaderCount },
			                      shaders);
			
			// Delete the shaders attached to the program
			for(shaderNum = 0; shaderNum < shaderCount; shaderNum++)
			{
				GL.DeleteShader(shaders[shaderNum]);
			}
			
			GL.DeleteProgram(prgName);
			GL.UseProgram(0);
		}
		
		void GetGLError()
		{
			var error = GL.GetError();
			while(error != ErrorCode.NoError)
			{
				Console.WriteLine("GLError " +  Enum.GetName(typeof(ErrorCode), error)); //GLUtil.GetGLErrorString(error));
			}
		}
		#endregion
	}
}

