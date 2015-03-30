using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using CoreGraphics;

namespace iOS
{
	partial class NewsWebViewController : TaskUIViewController
	{
        public string DisplayUrl { get; set; }

		public NewsWebViewController ( ) : base ( )
		{
		}

        UIWebView WebView { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            WebView = new UIWebView( );
            WebView.Layer.AnchorPoint = CGPoint.Empty;
            View.AddSubview( WebView );
            WebView.LoadRequest( new NSUrlRequest( new NSUrl( DisplayUrl ) ) );
        }

        public override void LayoutChanged()
        {
            base.LayoutChanged();

            WebView.Bounds = View.Bounds;
        }
	}
}
