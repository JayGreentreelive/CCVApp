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
		UITextField EmailField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField FirstNameField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel HeaderLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField LastNameField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton LogOutButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField MiddleNameField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField NickNameField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton SubmitButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField TitleField { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (EmailField != null) {
				EmailField.Dispose ();
				EmailField = null;
			}
			if (FirstNameField != null) {
				FirstNameField.Dispose ();
				FirstNameField = null;
			}
			if (HeaderLabel != null) {
				HeaderLabel.Dispose ();
				HeaderLabel = null;
			}
			if (LastNameField != null) {
				LastNameField.Dispose ();
				LastNameField = null;
			}
			if (LogOutButton != null) {
				LogOutButton.Dispose ();
				LogOutButton = null;
			}
			if (MiddleNameField != null) {
				MiddleNameField.Dispose ();
				MiddleNameField = null;
			}
			if (NickNameField != null) {
				NickNameField.Dispose ();
				NickNameField = null;
			}
			if (SubmitButton != null) {
				SubmitButton.Dispose ();
				SubmitButton = null;
			}
			if (TitleField != null) {
				TitleField.Dispose ();
				TitleField = null;
			}
		}
	}
}
