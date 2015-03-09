// This file has been autogenerated from a class added in the UI designer.

using System;

using Foundation;
using UIKit;

namespace iOS
{
	public class NotesWebViewController : TaskUIViewController
	{
        public string ActiveUrl { get; set; }

        UIWebView WebView { get; set; }
        UIActivityIndicatorView Indicator { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            WebView = new UIWebView( View.Frame );
            View.AddSubview( WebView );
            WebView.LoadRequest( new NSUrlRequest( new NSUrl( ActiveUrl ) ) );

            // place a busy indicator
            Indicator = new UIActivityIndicatorView( );
            Indicator.Layer.Position = new CoreGraphics.CGPoint( View.Frame.Width / 2, View.Frame.Height / 2 );
            Indicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
            Indicator.Color = UIColor.Gray;
            Indicator.StartAnimating( );
            WebView.AddSubview( Indicator );

            Indicator.Hidden = false;

            // once the page is done loading, hide the indicator
            WebView.LoadFinished += (object sender, EventArgs e ) =>
            {
                Indicator.Hidden = true;
            };
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            WebView.Frame = View.Frame;
            Indicator.Layer.Position = new CoreGraphics.CGPoint( View.Frame.Width / 2, View.Frame.Height / 2 );

            UIApplication.SharedApplication.IdleTimerDisabled = true;
            Console.WriteLine( "Turning idle timer OFF" );
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            UIApplication.SharedApplication.IdleTimerDisabled = true;
            Console.WriteLine( "Turning idle timer OFF" );
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            UIApplication.SharedApplication.IdleTimerDisabled = false;
            Console.WriteLine( "Turning idle timer ON" );
        }

        public override void OnActivated()
        {
            base.OnActivated();

            UIApplication.SharedApplication.IdleTimerDisabled = true;
            Console.WriteLine( "Turning idle timer OFF" );
        }

        public override void WillEnterForeground()
        {
            base.WillEnterForeground();

            UIApplication.SharedApplication.IdleTimerDisabled = true;
            Console.WriteLine( "Turning idle timer OFF" );
        }

        public override void AppOnResignActive()
        {
            base.AppOnResignActive( );

            UIApplication.SharedApplication.IdleTimerDisabled = false;
            Console.WriteLine( "Turning idle timer ON" );
        }

        public override void AppWillTerminate()
        {
            base.AppWillTerminate( );

            UIApplication.SharedApplication.IdleTimerDisabled = false;
            Console.WriteLine( "Turning idle timer ON" );
        }
	}
}
