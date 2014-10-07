// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;

namespace iOS
{
	[Register ("AboutMainPageUIViewController")]
	partial class AboutMainPageUIViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel AboutVersionText { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (AboutVersionText != null) {
				AboutVersionText.Dispose ();
				AboutVersionText = null;
			}
		}
	}
}
