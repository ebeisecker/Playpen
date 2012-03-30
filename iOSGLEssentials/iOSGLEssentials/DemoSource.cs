using System;
using OpenTK.Graphics.ES20;
using System.IO;

namespace iOSGLEssentials
{
	public class DemoSource
	{
		public string String{get; set;}
		public int ByteSize {get; set;}
		public All ShaderType {get; set;} // Vertex or Fragment
		
		public DemoSource ()
		{
		}
		
		public static DemoSource LoadSource(string filePathName)
		{
			var source = new DemoSource();
			
			// Check the file name suffix to determine what type of shader this is
			if(Path.GetExtension(filePathName) == ".fsh")
			{
				source.ShaderType = All.FragmentShader;
			}
			else if(Path.GetExtension(filePathName) == ".vsh")
			{
				source.ShaderType = All.VertexShader;
			}
			else
			{
				// Unknown
				source.ShaderType = (All)0;
			}
			
			var fileContent = File.ReadAllText(filePathName);
			source.String = fileContent;
			
			return source;
		}
	}
}

