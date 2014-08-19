using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using MonoTouch.CoreAnimation;
using System.Drawing;

namespace iOS
{
	partial class SpringboardViewController : UIViewController
	{
        MainUINavigationController NavViewController { get; set; }

        AboutActivity About { get; set; }
        NewsActivity News { get; set; }
        NotesActivity Notes { get; set; }

		public SpringboardViewController (IntPtr handle) : base (handle)
		{
            NavViewController = Storyboard.InstantiateViewController( "MainUINavigationController" ) as MainUINavigationController;

            // Instantiate all activities
            About = new AboutActivity( "AboutStoryboard_iPhone" );
            News = new NewsActivity( "NewsStoryboard_iPhone" );
            Notes = new NotesActivity( "" );
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad( );

            View.BackgroundColor = UIColor.Black;

            // set our image up
            string imagePath = NSBundle.MainBundle.BundlePath + "/me.jpg";

            ProfileImage.Layer.Contents = new UIImage( imagePath ).CGImage;

            CALayer maskLayer = new CALayer();
            maskLayer.AnchorPoint = new PointF( 0, 0 );
            maskLayer.Bounds = ProfileImage.Layer.Bounds;
            maskLayer.CornerRadius = ProfileImage.Layer.Bounds.Width / 2;
            maskLayer.BackgroundColor = UIColor.Black.CGColor;
            ProfileImage.Layer.Mask = maskLayer;
            //

            NewsButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    NavViewController.ActivateActivity( News );
                };

            SermonNotesButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    NavViewController.ActivateActivity( Notes );
                };

            AboutCCVButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    NavViewController.ActivateActivity( About );
                };

            AddChildViewController( NavViewController );
            View.AddSubview( NavViewController.View );

            SetNeedsStatusBarAppearanceUpdate( );
        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            NavViewController.RevealSpringboard( false );
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // always start up the app with the news visible
            NavViewController.ActivateActivity( About );
        }

        public void OnResignActive( )
        {
            NavViewController.OnResignActive( );
        }

        public void DidEnterBackground( )
        {
            NavViewController.DidEnterBackground( );
        }

        public void WillTerminate( )
        {
            NavViewController.WillTerminate( );
        }
	}
}
