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
	[Register ("NewsDetailsUIViewController")]
	partial class NewsDetailsUIViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel NewsDescriptionLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel NewsTitleLabel { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (NewsDescriptionLabel != null) {
				NewsDescriptionLabel.Dispose ();
				NewsDescriptionLabel = null;
			}
			if (NewsTitleLabel != null) {
				NewsTitleLabel.Dispose ();
				NewsTitleLabel = null;
			}
		}
	}
}
