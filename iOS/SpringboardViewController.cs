using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;

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
            NavViewController.ActivateActivity( News );
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
