using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using CoreGraphics;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;

namespace iOS
{
	partial class AboutMainPageUIViewController : TaskUIViewController
	{
		public AboutMainPageUIViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CGRect webViewFrame;
            AboutVersionText.Text = string.Format( "Version: {0}", BuildStrings.Version );

            webViewFrame = new CGRect( View.Frame.X, 
                                           View.Frame.Y + AboutVersionText.Frame.Height, 
                                           View.Frame.Width, 
                                           View.Frame.Height - AboutVersionText.Frame.Height );

            UIWebView webView = new UIWebView( webViewFrame );
            View.AddSubview( webView );
            webView.LoadRequest( new NSUrlRequest( new NSUrl( AboutConfig.Url ) ) );
        }
	}
}
