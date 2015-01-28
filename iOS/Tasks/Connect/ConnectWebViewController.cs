using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;

namespace iOS
{
	partial class ConnectWebViewController : TaskUIViewController
	{
        public string DisplayUrl { get; set; }

		public ConnectWebViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            UIWebView webView = new UIWebView( View.Frame );
            View.AddSubview( webView );
            webView.LoadRequest( new NSUrlRequest( new NSUrl( DisplayUrl ) ) );
        }
	}
}
