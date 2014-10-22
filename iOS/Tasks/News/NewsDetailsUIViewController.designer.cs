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
		UIImageView ImageBanner { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton LearnMoreButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel NewsDescriptionLabel { get; set; }

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
			if (NewsDescriptionLabel != null) {
				NewsDescriptionLabel.Dispose ();
				NewsDescriptionLabel = null;
			}
		}
	}
}
