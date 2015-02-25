using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace iOS
{
	partial class NewsWebViewController : TaskUIViewController
	{
        public string DisplayUrl { get; set; }

		public NewsWebViewController (IntPtr handle) : base (handle)
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
