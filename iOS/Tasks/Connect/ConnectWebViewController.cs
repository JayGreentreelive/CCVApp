using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;

namespace iOS
{
	partial class ConnectWebViewController : TaskUIViewController
	{
        public string DisplayUrl { get; set; }

        UIWebView WebView { get; set; }

		public ConnectWebViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            WebView = new UIWebView( );
            WebView.Frame = View.Frame;
            View.AddSubview( WebView );
            WebView.LoadRequest( new NSUrlRequest( new NSUrl( DisplayUrl ) ) );
        }

        public override void LayoutChanged()
        {
            base.LayoutChanged();

            WebView.Frame = View.Frame;
        }
	}
}
