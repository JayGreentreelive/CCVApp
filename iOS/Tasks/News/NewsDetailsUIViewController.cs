using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using CCVApp.Shared.Network;
using CoreGraphics;
using CCVApp.Shared.Config;
using Rock.Mobile.PlatformUI;
using CCVApp.Shared.Strings;

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

            View.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            // populate the details view with this news item.
            NewsDescription.Text = NewsItem.Description;
            NewsDescription.BackgroundColor = UIColor.Clear;
            NewsDescription.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );
            NewsDescription.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Small_Font_Light, ControlStylingConfig.Small_FontSize );
            NewsDescription.TextContainerInset = UIEdgeInsets.Zero;
            NewsDescription.TextContainer.LineFragmentPadding = 0;

            if( string.IsNullOrEmpty( NewsItem.HeaderImageName ) == false )
            {
                ImageBanner.Image = new UIImage( NSBundle.MainBundle.BundlePath + "/" + NewsItem.HeaderImageName );
                ImageBanner.ContentMode = UIViewContentMode.Center;
            }
            else
            {
                ImageBanner.Image = new UIImage( NSBundle.MainBundle.BundlePath + "/" + "podcastThumbnailPlaceholder.png" );
            }
            ImageBanner.BackgroundColor = UIColor.Green;

            LearnMoreButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    UIApplication.SharedApplication.OpenUrl( new NSUrl( NewsItem.ReferenceURL ) );
                };

            ControlStyling.StyleButton( LearnMoreButton, NewsStrings.LearnMore, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );

            ControlStyling.StyleUILabel( NewsTitle, ControlStylingConfig.Large_Font_Bold, ControlStylingConfig.Large_FontSize );
            NewsTitle.Text = NewsItem.Title;
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // if the description needs to scroll, enable user interaction (which disables the nav toolbar)
            if ( NewsDescription.ContentSize.Height > NewsDescription.Frame.Height )
            {
                NewsDescription.UserInteractionEnabled = true;
            }
            else
            {
                NewsDescription.UserInteractionEnabled = false;
            }

            // adjust the news title to have padding on the left and right.
            NewsTitle.Layer.AnchorPoint = CGPoint.Empty;
            NewsTitle.SizeToFit( );
            NewsTitle.Frame = new CGRect( NewsDescription.Frame.Left, ImageBanner.Frame.Bottom + 10, View.Bounds.Width - 30, NewsTitle.Bounds.Height );
        }
	}
}
