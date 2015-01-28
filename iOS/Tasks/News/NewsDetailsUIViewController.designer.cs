// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace iOS
{
	[Register ("NewsDetailsUIViewController")]
	partial class NewsDetailsUIViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIImageView ImageBanner { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton LearnMoreButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextView NewsDescription { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel NewsTitle { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (ImageBanner != null) {
				ImageBanner.Dispose ();
				ImageBanner = null;
			}
			if (LearnMoreButton != null) {
				LearnMoreButton.Dispose ();
				LearnMoreButton = null;
			}
			if (NewsDescription != null) {
				NewsDescription.Dispose ();
				NewsDescription = null;
			}
			if (NewsTitle != null) {
				NewsTitle.Dispose ();
				NewsTitle = null;
			}
		}
	}
}
