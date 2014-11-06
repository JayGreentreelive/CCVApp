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
	[Register ("NotesDetailsUIViewController")]
	partial class NotesDetailsUIViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView SeriesTable { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (SeriesTable != null) {
				SeriesTable.Dispose ();
				SeriesTable = null;
			}
		}
	}
}
