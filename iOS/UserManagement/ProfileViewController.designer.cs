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
	[Register ("ProfileViewController")]
	partial class ProfileViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField CellPhoneField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField EmailField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField LastNameField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton LogOutButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField NickNameField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton ProfilePicButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton SubmitButton { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (CellPhoneField != null) {
				CellPhoneField.Dispose ();
				CellPhoneField = null;
			}
			if (EmailField != null) {
				EmailField.Dispose ();
				EmailField = null;
			}
			if (LastNameField != null) {
				LastNameField.Dispose ();
				LastNameField = null;
			}
			if (LogOutButton != null) {
				LogOutButton.Dispose ();
				LogOutButton = null;
			}
			if (NickNameField != null) {
				NickNameField.Dispose ();
				NickNameField = null;
			}
			if (ProfilePicButton != null) {
				ProfilePicButton.Dispose ();
				ProfilePicButton = null;
			}
			if (SubmitButton != null) {
				SubmitButton.Dispose ();
				SubmitButton = null;
			}
		}
	}
}
