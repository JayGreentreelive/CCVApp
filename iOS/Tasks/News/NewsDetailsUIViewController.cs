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
            //NewsTitleLabel.Text = NewsItem.Title;
            NewsDescriptionLabel.Text = NewsItem.Description;

            if( string.IsNullOrEmpty( NewsItem.HeaderImageName ) == false )
            {
                ImageBanner.Image = new UIImage( NSBundle.MainBundle.BundlePath + "/" + NewsItem.HeaderImageName );
            }
            else
            {
                ImageBanner.Image = null;
            }

            LearnMoreButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    UIApplication.SharedApplication.OpenUrl( new NSUrl( NewsItem.ReferenceURL ) );
                };
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
        }
	}
}
