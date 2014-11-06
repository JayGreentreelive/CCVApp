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
	[Register ("NotesMainUIViewController")]
	partial class NotesMainUIViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView NotesTableView { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (NotesTableView != null) {
				NotesTableView.Dispose ();
				NotesTableView = null;
			}
		}
	}
}
