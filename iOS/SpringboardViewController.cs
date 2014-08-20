using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using MonoTouch.CoreAnimation;
using System.Drawing;
using System.Collections.Generic;

namespace iOS
{
	partial class SpringboardViewController : UIViewController
	{
        MainUINavigationController NavViewController { get; set; }

        /// <summary>
        /// Represents a selectable element on the springboard. 
        /// Contains its button and the associated activity.
        /// </summary>
        protected class SpringboardElement
        {
            public SpringboardViewController SpringboardViewController { get; set; }

            public Activity Activity { get; set; }
            public UIButton Button { get; set; }

            public SpringboardElement( SpringboardViewController controller, Activity activity, UIButton button )
            {
                SpringboardViewController = controller;
                Activity = activity;
                Button = button;

                Button.TouchUpInside += (object sender, EventArgs e) => 
                    {
                        SpringboardViewController.ActivateElement( this );
                    };
            }
        };

        /// <summary>
        /// A list of all the elements on the springboard page.
        /// </summary>
        /// <value>The elements.</value>
        protected List<SpringboardElement> Elements { get; set; }

		public SpringboardViewController (IntPtr handle) : base (handle)
		{
            NavViewController = Storyboard.InstantiateViewController( "MainUINavigationController" ) as MainUINavigationController;
            Elements = new List<SpringboardElement>( );
		}

        public override bool ShouldAutorotate()
        {
            switch( UIDevice.CurrentDevice.Orientation )
            {
                case UIDeviceOrientation.Portrait:
                {
                    NavViewController.SetNavigationBarHidden( false, true );
                    return true;
                }

                case UIDeviceOrientation.LandscapeLeft:
                case UIDeviceOrientation.LandscapeRight:
                {
                    // only allow landscape for the notes.
                    if( (NavViewController.CurrentActivity as NotesActivity) != null && NavViewController.IsSpringboardClosed( ) )
                    {
                        NavViewController.SetNavigationBarHidden( true, true );
                        return true;
                    }
                    return false;
                }
            }

            return false;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad( );

            // Instantiate all activities
            Elements.Add( new SpringboardElement( this, new NewsActivity( "NewsStoryboard_iPhone" )  , NewsButton ) );
            Elements.Add( new SpringboardElement( this, new NotesActivity( "" )                      , SermonNotesButton ) );
            Elements.Add( new SpringboardElement( this, new GiveActivity( "GiveStoryboard_iPhone" ), GroupFinderButton ) );//todo: Implement
            Elements.Add( new SpringboardElement( this, new GiveActivity( "GiveStoryboard_iPhone" ), PrayerButton ) );//todo: Implement
            Elements.Add( new SpringboardElement( this, new GiveActivity( "GiveStoryboard_iPhone" ), WatchButton ) );//todo: Implement
            Elements.Add( new SpringboardElement( this, new GiveActivity( "GiveStoryboard_iPhone" ), GiveButton ) );//todo: Implement
            Elements.Add( new SpringboardElement( this, new AboutActivity( "AboutStoryboard_iPhone" ), AboutCCVButton ) );

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


            AddChildViewController( NavViewController );
            View.AddSubview( NavViewController.View );

            SetNeedsStatusBarAppearanceUpdate( );
        }

        protected void ActivateElement( SpringboardElement activeElement )
        {
            foreach( SpringboardElement element in Elements )
            {
                if( element != activeElement )
                {
                    element.Button.BackgroundColor = UIColor.Clear;
                }
            }

            NavViewController.ActivateActivity( activeElement.Activity );
            activeElement.Button.BackgroundColor = RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( 0x7a1315FF);
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

            // start up the app with the first element
            ActivateElement( Elements[0] );
        }

        public void OnActivated( )
        {
            NavViewController.OnActivated( );
        }

        public void WillEnterForeground( )
        {
            NavViewController.WillEnterForeground( );
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
