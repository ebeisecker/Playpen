// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace iOSGLEssentials
{
	[Register ("iOSGLEssentialsViewController")]
	partial class iOSGLEssentialsViewController
	{
		[Outlet]
		iOSGLEssentials.EAGLView glView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (glView != null) {
				glView.Dispose ();
				glView = null;
			}
		}
	}
}
