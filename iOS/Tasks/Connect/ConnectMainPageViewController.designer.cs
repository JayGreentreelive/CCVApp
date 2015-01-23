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
	[Register ("ConnectMainPageViewController")]
	partial class ConnectMainPageViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView ConnectTableView { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (ConnectTableView != null) {
				ConnectTableView.Dispose ();
				ConnectTableView = null;
			}
		}
	}
}
