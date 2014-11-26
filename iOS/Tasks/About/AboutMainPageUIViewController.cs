using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using System.Drawing;
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

            RectangleF webViewFrame;
            AboutVersionText.Text = string.Format( "Version: {0}", BuildStrings.Version );

            webViewFrame = new RectangleF( View.Frame.X, 
                                           View.Frame.Y + AboutVersionText.Frame.Height, 
                                           View.Frame.Width, 
                                           View.Frame.Height - AboutVersionText.Frame.Height );

            UIWebView webView = new UIWebView( webViewFrame );
            View.AddSubview( webView );
            webView.LoadRequest( new NSUrlRequest( new NSUrl( AboutConfig.Url ) ) );
        }
	}
}
