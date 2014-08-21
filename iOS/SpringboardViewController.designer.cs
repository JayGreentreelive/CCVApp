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
	[Register ("SpringboardViewController")]
	partial class SpringboardViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton AboutButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton EpisodesButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton GroupFinderButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton NewsButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton PrayerButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView ProfileImage { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (AboutButton != null) {
				AboutButton.Dispose ();
				AboutButton = null;
			}
			if (EpisodesButton != null) {
				EpisodesButton.Dispose ();
				EpisodesButton = null;
			}
			if (GroupFinderButton != null) {
				GroupFinderButton.Dispose ();
				GroupFinderButton = null;
			}
			if (NewsButton != null) {
				NewsButton.Dispose ();
				NewsButton = null;
			}
			if (PrayerButton != null) {
				PrayerButton.Dispose ();
				PrayerButton = null;
			}
			if (ProfileImage != null) {
				ProfileImage.Dispose ();
				ProfileImage = null;
			}
		}
	}
}
