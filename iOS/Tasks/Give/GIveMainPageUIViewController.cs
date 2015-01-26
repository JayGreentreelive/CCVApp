using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using MonoTouch.AVFoundation;
using MonoTouch.MediaPlayer;
using CCVApp.Shared.Config;

namespace iOS
{
	partial class GIveMainPageUIViewController : TaskUIViewController
	{
        UIButton GiveButton { get; set; }

		public GIveMainPageUIViewController (IntPtr handle) : base (handle)
		{

		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ControlStyling.StyleBGLayer( GiveBannerLayer );

            GiveBanner.Text = CCVApp.Shared.Strings.GiveStrings.Header;
            ControlStyling.StyleUILabel( GiveBanner, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );

            GiveButton = UIButton.FromType( UIButtonType.Custom );
            GiveButton.TouchUpInside += (object sender, EventArgs e ) =>
            {
                UIApplication.SharedApplication.OpenUrl( new NSUrl( GiveConfig.GiveUrl ) );
            };
            ControlStyling.StyleButton( GiveButton, CCVApp.Shared.Strings.GiveStrings.ButtonLabel, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );

            GiveButton.SizeToFit( );
            GiveButton.Frame = new System.Drawing.RectangleF( ( View.Bounds.Width - GiveButton.Bounds.Width ) / 2, ( View.Bounds.Height - GiveButton.Bounds.Height ) / 2, GiveButton.Bounds.Width, GiveButton.Bounds.Height );
            View.AddSubview( GiveButton );
        }
	}
}
