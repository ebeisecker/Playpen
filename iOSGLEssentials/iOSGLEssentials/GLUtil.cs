using System;
using OpenTK.Graphics.ES20;

namespace iOSGLEssentials
{
	public static class GLUtil
	{
		public static string GetGLErrorString(All error)
		{
			var errorString = Enum.GetName(typeof(All), error);
			return errorString;
		}
	}
}

