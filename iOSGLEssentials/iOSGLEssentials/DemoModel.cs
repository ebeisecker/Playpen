using System;
using OpenTK.Graphics.ES20;
using System.IO;
using System.Runtime.InteropServices;

namespace iOSGLEssentials
{
	public class DemoModel
	{
		[StructLayout(LayoutKind.Explicit)]
		struct ModelHeader
		{
			[FieldOffset(0)]
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
			public string FileIdentifier;
			
			[FieldOffset(32)]
			public uint MajorVersion;
			
			[FieldOffset(36)]
			public uint MinorVersion;
		}
		
		[StructLayout(LayoutKind.Explicit)]
		struct ModelTOC
		{
			[FieldOffset(0)]
			public int AttribHeaderSize;
			[FieldOffset(4)]
			public int ByteElementOffset;
			[FieldOffset(8)]
			public int BytePositionOffset;
			[FieldOffset(12)]
			public int ByteTexcoordOffset;
			[FieldOffset(16)]
			public int ByteNormalOffset;
		}
		
		[StructLayout(LayoutKind.Explicit)]
		struct ModelAttrib
		{
			[FieldOffset(0)]
			public int ByteSize;
			[FieldOffset(4)]
			public int DataType;
			[FieldOffset(8)]
			public All PrimType;
			[FieldOffset(12)]
			public int SizePerElement;
			[FieldOffset(16)]
			public int NumElements;
		}
		
		#region Properties
		public int NumVertcies { get;set; }
		
		public byte[] Positions { get; set; }
		public VertexAttribPointerType PositionType { get; set; }
		public int PositionSize { get; set; }
		public int PositionArraySize { get; set; }
		
		public byte[] TexCoords { get; set; }
		public VertexAttribPointerType TexCoordType {get; set;}
		public int TexCoordsSize { get; set; }
		public int TexCoordArraySize { get; set; }
		
		public byte[] Normals {get; set;}
		public VertexAttribPointerType NormalType {get; set;}
		public int NormalSize {get; set;}
		public int NormalArraySize {get; set;}
		
		public byte[] Elements {get; set;}
		public DrawElementsType ElementType { get; set;}
		public int NumElements {get; set;}
		public int ElementArraySize {get; set;}
		
		public All PrimType {get; set;}
		#endregion
		
		public DemoModel ()
		{
			
		}
		
		static T ToStruct<T>(byte[] buffer)
		{
			var handel = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			var strct = (T)Marshal.PtrToStructure(handel.AddrOfPinnedObject(), typeof(T));
			handel.Free();
			return strct;
		}
		
		public static DemoModel LoadModel(string filePathName)
		{
			if(string.IsNullOrEmpty(filePathName))
			{
				throw new ArgumentNullException("filePathName");
			}
			
			var model = new DemoModel();
			
			using(var fileStream = File.Open(filePathName, FileMode.Open))
			using(var binaryReader = new BinaryReader(fileStream))
			{	
				// Get the Size of the ModelHeader
				var size = Marshal.SizeOf(typeof(ModelHeader));
				var buffer = new byte[size];
				
				// Create the ModelHeader from the bytes Read in.
				buffer = binaryReader.ReadBytes(size);
				var modelHeader = ToStruct<ModelHeader>(buffer);
				
				if(modelHeader.FileIdentifier.CompareTo("AppleOpenGLDemoModelWWDC2010") != 0)
				{
					Console.WriteLine("File Identifier incorrect");
					return null;
				}
				
				if(modelHeader.MajorVersion != 0 && modelHeader.MinorVersion != 1)
				{
					Console.WriteLine("Model Version Incorrect");
					return null;
				}
				
				//
				// Read in the Model Elements Data
				//
				
				// Get the size of the ModelTOC
				size = Marshal.SizeOf(typeof(ModelTOC));
				Array.Resize<byte>(ref buffer, size);
				
				// Convert the Bytes into the Struct
				buffer = binaryReader.ReadBytes(size);
				var modelTOC = ToStruct<ModelTOC>(buffer);
				
				if(modelTOC.AttribHeaderSize > Marshal.SizeOf (typeof(ModelAttrib)))
				{
					Console.WriteLine("Model Attribute Size incorrect");
					return null;
				}
				
				fileStream.Seek(modelTOC.ByteElementOffset, SeekOrigin.Begin);
				
				// Get the size of the ModelAttrib
				size = Marshal.SizeOf(typeof(ModelAttrib));
				Array.Resize<byte>(ref buffer, size);
				
				// Convert the bytes into the struct
				buffer = binaryReader.ReadBytes(size);
				var attrib = ToStruct<ModelAttrib>(buffer);
				
				model.ElementArraySize = attrib.ByteSize;
				model.ElementType = (DrawElementsType)Enum.ToObject(typeof(DrawElementsType), attrib.DataType);
				model.NumElements = attrib.NumElements;
				
				// OpenGL ES cannot use uint element
				// So if the model has UI elements
				/*
				if(model.ElementType != DrawElementsType.UnsignedShort)
				{
					// ...Load the UI elements and convert to UnsignedShort
					
					var uiElements = new byte[model.ElementArraySize];
					model.Elements = new byte[model.NumElements * Marshal.SizeOf (typeof(short))];
					
					uiElements = binaryReader.ReadBytes(model.ElementArraySize);
					
					var elemNum = 0;
					for(elemNum = 0; elemNum < model.NumElements; elemNum++)
					{
						// We can't hanle this model if an element is out of the UnsignedInt range
						if(Convert.ToUInt32 (uiElements[elemNum]) >= 0xFFFF)
						{
							return null;	
						}
						
						model.Elements[elemNum] = uiElements[elemNum];
					}
					
					model.ElementType = DrawElementsType.UnsignedShort;
					model.ElementArraySize = model.NumElements * Marshal.SizeOf(typeof(short));
				}
				
				else
				{*/
					model.Elements = new byte[model.ElementArraySize];				
					model.Elements = binaryReader.ReadBytes(model.ElementArraySize);
				//}
				
				//
				// Read the models Position Data
				//
				
				binaryReader.BaseStream.Seek (modelTOC.BytePositionOffset, SeekOrigin.Begin);
				
				// Convert file bytes to Struct
				Array.Resize<byte>(ref buffer, modelTOC.AttribHeaderSize);
				buffer = binaryReader.ReadBytes(modelTOC.AttribHeaderSize);
				attrib = ToStruct<ModelAttrib>(buffer);
				
				model.PositionArraySize = attrib.ByteSize;
				model.PositionType = (VertexAttribPointerType)Enum.ToObject(typeof(VertexAttribPointerType), attrib.DataType);
				model.PositionSize = attrib.SizePerElement;
				model.NumVertcies = attrib.NumElements;
				model.Positions = new byte[model.PositionArraySize];
				
				model.Positions = binaryReader.ReadBytes(model.PositionArraySize);
				
				//
				// Read the models Texture Data
				// 
				
				fileStream.Seek(modelTOC.ByteTexcoordOffset, SeekOrigin.Begin);
				
				// Convert file bytes to Struct
				// Array.Resize<byte>(ref buffer, modelTOC.AttribHeaderSize);
				buffer = binaryReader.ReadBytes(modelTOC.AttribHeaderSize);
				attrib = ToStruct<ModelAttrib>(buffer);
				
				model.TexCoordArraySize = attrib.ByteSize;
				model.TexCoordType = (VertexAttribPointerType)Enum.ToObject(typeof(VertexAttribPointerType), attrib.DataType);
				model.TexCoordsSize = attrib.SizePerElement;
				
				// Must have the same number of texcoords as positions
				if(model.NumVertcies != attrib.NumElements)
				{
					Console.WriteLine("Must have the same number of texcoords as positions");
					return null;
				}
				
				model.TexCoords = new byte[model.TexCoordArraySize];
				
				//
				// Read the models Normals Data
				//
				fileStream.Seek(modelTOC.ByteNormalOffset, SeekOrigin.Begin);
				
				// Convert file bytes to Struct
				buffer = binaryReader.ReadBytes(modelTOC.AttribHeaderSize);
				attrib = ToStruct<ModelAttrib>(buffer);
				
				model.NormalArraySize = attrib.ByteSize;
				model.NormalType = (VertexAttribPointerType)Enum.ToObject(typeof(VertexAttribPointerType), attrib.DataType);
				model.NormalSize = attrib.SizePerElement;
				model.Normals = binaryReader.ReadBytes(model.NormalArraySize);
				
				return model;
			}
		}	
		
		public static DemoModel LoadQuadModel()
		{
		 	var posArray = new float[]
			{
				-200.0f, 0.0f, -200.0f,
				 200.0f, 0.0f, -200.0f,
				 200.0f, 0.0f,  200.0f,
				-200.0f, 0.0f,  200.0f
			};
				
			var texcoordArray = new float[]
			{ 
				0.0f,  1.0f,
				1.0f,  1.0f,
				1.0f,  0.0f,
				0.0f,  0.0f
			};
			
			var normalArray = new float[]
			{
				0.0f, 0.0f, 1.0f,
				0.0f, 0.0f, 1.0f,
				0.0f, 0.0f, 1.0f,
				0.0f, 0.0f, 1.0f,
			};
			
			var elementArray = new short[]
			{
				0, 2, 1,
				0, 3, 2
			};
			
			var model = new DemoModel();
			
			model.PositionType = VertexAttribPointerType.Float;
			model.PositionSize = 3;
			model.PositionArraySize = posArray.Length * 4;
			model.Positions = new byte[model.PositionArraySize];
			Buffer.BlockCopy(posArray, 0, model.Positions, 0, model.PositionArraySize);
			
			model.TexCoordType = VertexAttribPointerType.Float;
			model.TexCoordsSize = 2;
			model.TexCoordArraySize = texcoordArray.Length * 4;
			model.TexCoords = new byte[model.PositionArraySize];
			Buffer.BlockCopy(texcoordArray, 0, model.TexCoords, 0, model.TexCoordArraySize);
			
			model.NormalType = VertexAttribPointerType.Float;
			model.NormalSize = 3;
			model.NormalArraySize = normalArray.Length * 4;
			model.Normals = new byte[model.NormalArraySize];
			Buffer.BlockCopy(normalArray, 0, model.Normals, 0, model.NormalArraySize);
			
			model.ElementArraySize = elementArray.Length * 2;
			model.Elements = new byte[model.ElementArraySize];
			Buffer.BlockCopy(elementArray, 0, model.Elements, 0, model.ElementArraySize);
			
			model.PrimType = All.Triangles;
			
			model.NumElements = elementArray.Length;
			model.ElementType = DrawElementsType.UnsignedShort;
			model.NumVertcies = model.PositionArraySize / (model.PositionSize * 4);
			
			return model;
		}
	}
}

