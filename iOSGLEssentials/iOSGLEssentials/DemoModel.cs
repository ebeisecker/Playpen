using System;
using OpenTK.Graphics.ES11;

namespace iOSGLEssentials
{
	public class DemoModel
	{
		struct ModelHeader
		{
			public string FileIdentifier{ get;set; }
			public uint MajorVersion { get; set; }
			public uint MinorVersion { get; set; }
		}
		
		#region Properties
		public int NumVertcies { get;set; }
		
		public byte[] Positions { get; set; }
		public All PositionType { get; set; }
		public int PositionSize { get; set; }
		public int PositionArraySize { get; set; }
		
		public byte[] TexCoords { get; set; }
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
		
		
	}
}

