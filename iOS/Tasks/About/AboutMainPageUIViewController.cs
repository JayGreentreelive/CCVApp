using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using System.Drawing;

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
            AboutVersionText.Text = string.Format( "CCV App Version {0}\nBuilt on {1}", CCVApp.Shared.Strings.Build.Version, CCVApp.Shared.Strings.Build.BuildTime );

            webViewFrame = new RectangleF( View.Frame.X, 
                                           View.Frame.Y + NavigationController.NavigationBar.Frame.Height + AboutVersionText.Frame.Height, 
                                           View.Frame.Width, 
                                           View.Frame.Height - NavigationController.NavigationBar.Frame.Height - AboutVersionText.Frame.Height );

            UIWebView webView = new UIWebView( webViewFrame );
            View.AddSubview( webView );
            webView.LoadRequest( new NSUrlRequest( new NSUrl( CCVApp.Shared.Config.About.Url ) ) );
        }
	}
}
