using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using CCVApp.Shared.Network;

namespace iOS
{
	partial class NewsDetailsUIViewController : TaskUIViewController
	{
        public RockNews NewsItem { get; set; }

		public NewsDetailsUIViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // populate the details view with this news item.
            NewsTitleLabel.Text = NewsItem.Title;
            NewsDescriptionLabel.Text = NewsItem.Description;
        }
	}
}
