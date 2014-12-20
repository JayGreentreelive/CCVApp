using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using CCVApp.Shared.Network;
using System.Drawing;
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

            View.BackgroundColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor );

            // populate the details view with this news item.
            NewsDescription.Text = NewsItem.Description;
            NewsDescription.BackgroundColor = UIColor.Clear;
            NewsDescription.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.Label_TextColor );
            NewsDescription.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( ControlStylingConfig.Small_Font_Light, ControlStylingConfig.Small_FontSize );
            NewsDescription.TextContainerInset = UIEdgeInsets.Zero;
            NewsDescription.TextContainer.LineFragmentPadding = 0;
            NewsDescription.UserInteractionEnabled = false;

            if( string.IsNullOrEmpty( NewsItem.HeaderImageName ) == false )
            {
                ImageBanner.Image = new UIImage( NSBundle.MainBundle.BundlePath + "/" + NewsItem.HeaderImageName );
                ImageBanner.ContentMode = UIViewContentMode.Center;
            }
            else
            {
                ImageBanner.Image = null;
            }
            ImageBanner.BackgroundColor = UIColor.Green;

            LearnMoreButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    UIApplication.SharedApplication.OpenUrl( new NSUrl( NewsItem.ReferenceURL ) );
                };

            ControlStyling.StyleButton( LearnMoreButton, NewsStrings.LearnMore, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );

            ControlStyling.StyleUILabel( NewsTitle, ControlStylingConfig.Large_Font_Bold, ControlStylingConfig.Large_FontSize );
            NewsTitle.Text = NewsItem.Title;
            NewsTitle.SizeToFit( );

        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
        }
	}
}
