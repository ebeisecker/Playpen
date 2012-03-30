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
			
			[FieldOffset(8)]
			public uint MajorVersion;
			
			[FieldOffset(12)]
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
			public All DataType;
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
		public All PositionType { get; set; }
		public int PositionSize { get; set; }
		public int PositionArraySize { get; set; }
		
		public byte[] TexCoords { get; set; }
		public All TexCoordType {get; set;}
		public int TexCoordsSize { get; set; }
		public int TexCoordArraySize { get; set; }
		
		public byte[] Normals {get; set;}
		public All NormalType {get; set;}
		public int NormalSize {get; set;}
		public int NormalArraySize {get; set;}
		
		public byte[] Elements {get; set;}
		public All ElementType { get; set;}
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
				return null;
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
				model.ElementType = attrib.DataType;
				model.NumElements = attrib.NumElements;
				
				// OpenGL ES cannot use uint element
				// So if the model has UI elements
				if(model.ElementType == All.UnsignedInt)
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
					
					model.ElementType = All.UnsignedShort;
					model.ElementArraySize = model.NumElements * Marshal.SizeOf(typeof(short));
				}
				else
				{
					model.Elements = new byte[model.ElementArraySize];				
					model.Elements = binaryReader.ReadBytes(model.ElementArraySize);
				}
				
				binaryReader.BaseStream.Seek (modelTOC.BytePositionOffset, SeekOrigin.Begin);
				
				// Convert file bytes to Struct
				Array.Resize<byte>(ref buffer, modelTOC.AttribHeaderSize);
				buffer = binaryReader.ReadBytes(modelTOC.AttribHeaderSize);
				attrib = ToStruct<ModelAttrib>(buffer);
				
				model.PositionArraySize = attrib.ByteSize;
				model.PositionType = attrib.DataType;
				model.PositionSize = attrib.SizePerElement;
				model.Positions = new byte[model.PositionArraySize];
				
				model.Positions = binaryReader.ReadBytes(model.PositionArraySize);
				
				fileStream.Seek(modelTOC.ByteTexcoordOffset, SeekOrigin.Begin);
				
				// Convert file bytes to Struct
				Array.Resize<byte>(ref buffer, modelTOC.AttribHeaderSize);
				buffer = binaryReader.ReadBytes(modelTOC.AttribHeaderSize);
				attrib = ToStruct<ModelAttrib>(buffer);
				
				model.TexCoordArraySize = attrib.ByteSize;
				model.TexCoordType = attrib.DataType;
				model.TexCoordsSize = attrib.SizePerElement;
				
				// Must have the same number of texcoords as positions
				if(model.NumVertcies != attrib.NumElements)
				{
					return null;
				}
				
				model.TexCoords = new byte[model.TexCoordArraySize];
				
				fileStream.Seek(modelTOC.ByteNormalOffset, SeekOrigin.Begin);
				
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
			
			model.PositionType = All.Float;
			model.PositionSize = 3;
			model.PositionArraySize = posArray.Length;
			model.Positions = new byte[model.PositionArraySize];
			Array.Copy(posArray, model.Positions, model.PositionArraySize);
			
			model.TexCoordType = All.Float;
			model.TexCoordsSize = 2;
			model.TexCoordArraySize = texcoordArray.Length;
			model.TexCoords = new byte[model.PositionArraySize];
			Array.Copy(texcoordArray, model.TexCoords, model.TexCoordArraySize);
			
			model.NormalType = All.Float;
			model.NormalSize = 3;
			model.NormalArraySize = normalArray.Length;
			model.Normals = new byte[model.NormalArraySize];
			Array.Copy(normalArray, model.Normals, model.NormalArraySize);
			
			model.ElementArraySize = elementArray.Length;
			model.Elements = new byte[model.ElementArraySize];
			Array.Copy(elementArray, model.Elements, model.ElementArraySize);
			
			model.PrimType = All.Triangles;
			
			model.NumElements = elementArray.Length;
			model.ElementType = All.UnsignedShort;
			model.NumVertcies = model.PositionArraySize / model.PositionSize;
			
			return model;
		}
	}
}

