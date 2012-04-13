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
using Java.Nio;
using Android.Util;
using Javax.Microedition.Khronos.Opengles;
using Javax.Microedition.Khronos.Egl;
using BasicGLSurfaceView;

namespace BasicGLSurface
{
	class GLES20TriangleRenderer : GLSurfaceView.IRenderer
	{
		#region Members
		private const int FLOAT_SIZE_BYTES = 4;
		private const int TRIANGLE_VERTICES_DATA_STRIDE_BYTES = 5 * FLOAT_SIZE_BYTES;
		private const int TRIANGLE_VERTICES_DATA_POS_OFFSET = 0;
		private const int TRIANGLE_VERTICES_DATA_UV_OFFSET = 3;
		private readonly float[] m_TriangleVerticesData = new float[] {
			// X, Y, Z, U, V
			-1.0f, -0.5f, 0, -0.5f, 0.0f,
			1.0f, -0.5f, 0, 1.5f, -0.0f, //???
			0.0f, 1.11803399f, 0, 0.5f, 1.61803399f };
		
		private FloatBuffer m_TriangleVertices;
		
		private const string m_VertexShader = "uniform mat4 uMVPMatrix;\n" +
										      "attribute vec4 aPosition;\n" +
										      "attribute vec2 aTextureCoord;\n" +
										      "varying vec2 vTextureCoord;\n" +
										      "void main() {\n" +
										      "  gl_Position = uMVPMatrix * aPosition;\n" +
										      "  vTextureCoord = aTextureCoord;\n" +
										      "}\n";
		
		private const string m_FragmentShader = "precision mediump float;\n" +
										        "varying vec2 vTextureCoord;\n" +
										        "uniform sampler2D sTexture;\n" +
										        "void main() {\n" +
										        "  gl_FragColor = texture2D(sTexture, vTextureCoord);\n" +
										        "}\n";
		
		private float[] m_MVPMatrix = new float[16];
		private float[] m_ProjMatrix = new float[16];
		private float[] m_MMatrix = new float[16];
		private float[] m_VMatrix = new float[16];
		
		private int m_Program;
		private int m_TextureID;
		private int m_uMVPMatrixHandle;
		private int m_aPositionHandle;
		private int m_aTextureHandle;
		
		private Context m_Context;
		private const string TAG = "GLES20TriangleRenderer";
		#endregion
		
		public GLES20TriangleRenderer(Context context)
		{
			m_Context = context;
			m_TriangleVertices = ByteBuffer.AllocateDirect(m_TriangleVerticesData.Length * 
			                                               FLOAT_SIZE_BYTES).Order(ByteOrder.NativeOrder()).AsFloatBuffer();
			m_TriangleVertices.Put(m_TriangleVerticesData).Position(0);
		}
		
		#region IJavaObject Members
		
		public IntPtr Handle
		{
			get { return this.Handle; }
		}
		
		#endregion
		
		#region IRenderer Members
		
		public void OnDrawFrame(IGL10 glUnused)
		{
			Log.Info(TAG, "OnDrawFrame");
			// Ignore the passed-in GL10 interface, and use the GLES20
			// class's static methods instead.
			GLES20.GlClearColor(0.0f, 0.0f, 1.0f, 1.0f);
			GLES20.GlClear(GLES20.GlDepthBufferBit | GLES20.GlColorBufferBit);
			GLES20.GlUseProgram(m_Program);
			CheckGLError("GlUseProgram");
			
			GLES20.GlActiveTexture(GLES20.GlTexture0);
			GLES20.GlBindTexture(GLES20.GlTexture2d, m_TextureID);
			
			m_TriangleVertices.Position(TRIANGLE_VERTICES_DATA_POS_OFFSET);
			GLES20.GlVertexAttribPointer(m_aPositionHandle, 3, GLES20.GlFloat, false,
			                             TRIANGLE_VERTICES_DATA_STRIDE_BYTES, m_TriangleVertices);
			CheckGLError("GlVertexAttribPointer m_aPosition");
			
			m_TriangleVertices.Position(TRIANGLE_VERTICES_DATA_UV_OFFSET);
			GLES20.GlEnableVertexAttribArray(m_aPositionHandle);
			CheckGLError("GlEnableVertexAttribArray m_aPositionHandle");
			
			GLES20.GlVertexAttribPointer(m_aTextureHandle, 2, GLES20.GlFloat, false,
			                             TRIANGLE_VERTICES_DATA_STRIDE_BYTES, m_TriangleVertices);
			CheckGLError("GlVertexAttribPointer m_aTextureHandle");
			
			GLES20.GlEnableVertexAttribArray(m_aTextureHandle);
			CheckGLError("GlEnableVertexAttribArray m_aTextureHandle");
			
			var time = DateTime.Now.Ticks % 40000L;
			var angle = 0.090f * ((int) time);
			Matrix.SetRotateM(m_MMatrix, 0, angle, 0, 0, 1.0f);
			Matrix.MultiplyMM(m_MVPMatrix, 0, m_VMatrix, 0, m_MMatrix, 0);
			Matrix.MultiplyMM(m_MVPMatrix, 0, m_ProjMatrix, 0, m_MVPMatrix, 0);
			
			GLES20.GlUniformMatrix4fv(m_uMVPMatrixHandle, 1, false, m_MVPMatrix, 0);
			GLES20.GlDrawArrays(GLES20.GlTriangles, 0, 3);
			CheckGLError("GlDrawArray");
		}
		
		public void OnSurfaceChanged(IGL10 glUnused, int width, int height)
		{
			Log.Info(TAG, "OnSurfaceChanged");
			// Ignore the passed-in GL10 interface, and use the GLES20
			// class's static methods instead.
			GLES20.GlViewport(0, 0, width, height);
			var ratio = (float) width / height;
			Matrix.FrustumM(m_ProjMatrix, 0, -ratio, ratio, -1, 1, 3, 7);
		}
		
		public void OnSurfaceCreated(IGL10 glUnused, EGLConfig config)
		{
			Log.Info(TAG, "OnSurfaceCreated");
			// Ignore the passed-in GL10 interface and use the GLES20
			// class's static methods instead
			m_Program = CreateProgram(m_VertexShader, m_FragmentShader);
			
			if(m_Program == 0)
			{
				return;
			}
			
			m_aPositionHandle = GLES20.GlGetAttribLocation(m_Program, "aPosition");
			CheckGLError("GlGetAttribLocation aPosition");
			if(m_aPositionHandle == -1)
			{
				throw new AndroidRuntimeException("Could not get attrib location for aPosition");
			}
			
			m_aTextureHandle = GLES20.GlGetAttribLocation(m_Program, "aTextureCoord");
			CheckGLError("GlGetAttribLocation aTextureCoord");
			if(m_aTextureHandle == -1)
			{
				throw new AndroidRuntimeException("Could not get attrib location for aTextureCoord");
			}
			
			m_uMVPMatrixHandle = GLES20.GlGetUniformLocation(m_Program, "uMVPMatrix");
			CheckGLError("GlGetUniformLocation uMVPMatrix");
			if(m_uMVPMatrixHandle == -1)
			{
				throw new AndroidRuntimeException("Could not get attrib location for uMVPMatrix");
			}
			
			// Create our texture. This has to be done each time the
			// surface is created.
			
			var textures = new int[1];
			GLES20.GlGenTextures(1, textures, 0);
			
			m_TextureID = textures[0];
			GLES20.GlBindTexture(GLES20.GlTexture2d, m_TextureID);
			
			GLES20.GlTexParameterf(GLES20.GlTexture2d, GLES20.GlTextureMinFilter, GLES20.GlNearest);
			GLES20.GlTexParameterf(GLES20.GlTexture2d, GLES20.GlTextureMagFilter, GLES20.GlLinear);
			
			GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapS, GLES20.GlRepeat);
			GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES10.GlTextureWrapT, GLES20.GlRepeat);
			
			Android.Graphics.Bitmap bitmap;
			
			using(var inStream = m_Context.Resources.OpenRawResource(Resource.Raw.robot))
			{
				bitmap = Android.Graphics.BitmapFactory.DecodeStream(inStream);
			}
			
			GLUtils.TexImage2D(GLES20.GlTexture2d, 0, bitmap, 0);
			bitmap.Recycle();
			
			Matrix.SetLookAtM(m_VMatrix, 0, 0, 0, -5, 0f, 0f, 0f, 0f, 1.0f, 0.0f);
		}
		
		public void Dispose()
		{
			
		}
		#endregion
		
		#region GL Helpers
		
		private int LoadShader(int shaderType, string source)
		{
			int shader = GLES20.GlCreateShader(shaderType);
			if(shader != 0)
			{
				GLES20.GlShaderSource (shader, source);
				GLES20.GlCompileShader(shader);
				
				var compiled = new int[1];
				GLES20.GlGetShaderiv(shader, GLES20.GlCompileStatus, compiled, 0);
				
				if(compiled[0] == 0)
				{
					Log.Error(TAG, "Could not compile shader " + shaderType + ":");
					Log.Error(TAG, GLES20.GlGetShaderInfoLog(shader));
					GLES20.GlDeleteShader(shader);
					shader = 0;
				}				
			}
			
			return shader;
		}		
		
		private int CreateProgram(string vertexSource, string fragmentSource)
		{
			var vertexShader = LoadShader(GLES20.GlVertexShader, vertexSource);
			if(vertexShader == 0)
			{
				return 0;
			}
			
			var pixelShader = LoadShader(GLES20.GlFragmentShader, fragmentSource);
			if(pixelShader == 0)
			{
				return 0;
			}
			
			var program = GLES20.GlCreateProgram();
			if(program != 0)
			{
				GLES20.GlAttachShader(program, vertexShader);
				CheckGLError("GlAttachShader");
				
				GLES20.GlAttachShader(program, pixelShader);
				CheckGLError("GlAttachShader");
				
				GLES20.GlLinkProgram(program);
				var linkStatus = new int[1];
				GLES20.GlGetProgramiv(program, GLES20.GlLinkStatus, linkStatus, 0);
				
				if(linkStatus[0] != GLES20.GlTrue)
				{
					Log.Error(TAG, "Could not link program: ");
					Log.Error(TAG, GLES20.GlGetProgramInfoLog(program));
					
					GLES20.GlDeleteProgram(program);
					program = 0;
				}
			}
			
			return program;
		}
		
		private void CheckGLError(string op)
		{
			int error;
			while((error = GLES20.GlGetError()) != GLES20.GlNoError)
			{
				var msg = op + ": GLError " + error;
				Log.Error(TAG, msg);
				throw new AndroidRuntimeException(msg);
			}
		}
		
		#endregion
	}
}

