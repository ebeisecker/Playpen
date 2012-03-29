using System;

namespace iOSGLEssentials
{
	public static class VectorUtil
	{
		public static float vec4DotProduct(float[] lhs, float[] rhs)
		{
			return lhs[0]*rhs[0] + lhs[1]*rhs[1] + lhs[2]*rhs[2] + lhs[3]*rhs[3];	
		}
	}
}

