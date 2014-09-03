using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using MonoTouch.CoreAnimation;
using System.Drawing;
using System.Collections.Generic;
using Rock.Mobile.Network;

namespace iOS
{
    /// <summary>
    /// The springboard acts as the core navigation for the user. From here
    /// they may launch any of the app's activities.
    /// </summary>
	partial class SpringboardViewController : UIViewController
	{
        MainUINavigationController NavViewController { get; set; }

        /// <summary>
        /// Represents a selectable element on the springboard.
        /// Contains its button and the associated task.
        /// </summary>
        protected class SpringboardElement
        {
            /// <summary>
            /// Reference to our parent view controller
            /// </summary>
            /// <value>The springboard view controller.</value>
            public SpringboardViewController SpringboardViewController { get; set; }

            /// <summary>
            /// The task that is launched by this element.
            /// </summary>
            /// <value>The task.</value>
            public Task Task { get; set; }

            /// <summary>
            /// The view that rests behind the button, graphic and text, and is colored when 
            /// the task is active.
            /// </summary>
            /// <value>The backing view.</value>
            public UIView BackingView { get; set; }

            /// <summary>
            /// The button itself. Because we have special display needs, we
            /// break the button apart, and this ends up being an empty container that lies
            /// on top of the BackingView, LogoView and TextView.
            /// </summary>
            /// <value>The button.</value>
            public UIButton Button { get; set; }

            public SpringboardElement( SpringboardViewController controller, Task task, UIButton button, string imageName )
            {
                UIView parentView = button.Superview;

                SpringboardViewController = controller;
                Task = task;
                Button = button;

                Button.TouchUpInside += (object sender, EventArgs e) => 
                    {
                        SpringboardViewController.ActivateElement( this );
                    };


                //The button should look as follows:
                // [ X Text ]
                // To make sure the icons and text are all aligned vertically,
                // we will actually create a backing view that can highlight (the []s)
                // and place a logo view (the X), and a text view (the Text) on top.
                // Finally, we'll make the button clear with no text and place it over the
                // backing view.

                // start by loading the image
                string imagePath = NSBundle.MainBundle.BundlePath + "/" + imageName;
                UIImage image = new UIImage( imagePath );

                // Create the backing view
                BackingView = new UIView( );
                BackingView.Frame = Button.Frame;
                BackingView.BackgroundColor = UIColor.Clear;
                parentView.AddSubview( BackingView );

                // Create the logo view containing the image.
                UIView logoView = new UIView( );
                logoView.Bounds = new RectangleF( 0, 0, image.Size.Width, image.Size.Height );
                logoView.Layer.Position = new PointF( CCVApp.Config.Springboard.Element_LogoOffsetX, Button.Layer.Position.Y );
                logoView.Layer.Contents = image.CGImage;
                logoView.BackgroundColor = UIColor.Clear;
                parentView.AddSubview( logoView );

                // Create the text, and populate it with the button's requested text, color and font.
                UILabel TextLabel = new UILabel( );
                TextLabel.Text = Button.Title( UIControlState.Normal );
                TextLabel.TextColor = Button.TitleColor( UIControlState.Normal );
                TextLabel.Font = Button.Font;
                TextLabel.BackgroundColor = UIColor.Clear;
                TextLabel.SizeToFit( );
                TextLabel.Layer.Position = new PointF( CCVApp.Config.Springboard.Element_LabelOffsetX + (TextLabel.Frame.Width / 2), Button.Layer.Position.Y );
                parentView.AddSubview( TextLabel );

                // now clear out the button so it just lays on top of the contents
                Button.SetTitle( "", UIControlState.Normal );
                Button.BackgroundColor = UIColor.Clear;

                parentView.BringSubviewToFront( Button );
            }
        };

        /// <summary>
        /// A list of all the elements on the springboard page.
        /// </summary>
        /// <value>The elements.</value>
        protected List<SpringboardElement> Elements { get; set; }

        protected UIDeviceOrientation CurrentOrientation { get; set; }

        protected LoginViewController ActiveLoginController { get; set; }

		public SpringboardViewController (IntPtr handle) : base (handle)
		{
            NavViewController = Storyboard.InstantiateViewController( "MainUINavigationController" ) as MainUINavigationController;
            Elements = new List<SpringboardElement>( );
		}

        public override bool ShouldAutorotate()
        {
            if( CurrentOrientation != UIDevice.CurrentDevice.Orientation )
            {
                // We only want to allow landscape orientation when in the NotesTask.
                // All other times the app should be in Portrait mode.
                switch( UIDevice.CurrentDevice.Orientation )
                {
                    case UIDeviceOrientation.Portrait:
                    {
                        CurrentOrientation = UIDevice.CurrentDevice.Orientation;

                        NavViewController.EnableSpringboardRevealButton( true );
                        return true;
                    }

                    case UIDeviceOrientation.LandscapeLeft:
                    case UIDeviceOrientation.LandscapeRight:
                    {
                        // only allow landscape for the notes.
                        if( (NavViewController.CurrentTask as NotesTask) != null && NavViewController.IsSpringboardClosed( ) )
                        {
                            CurrentOrientation = UIDevice.CurrentDevice.Orientation;

                            NavViewController.EnableSpringboardRevealButton( false );

                            return true;
                        }
                        return false;
                    }
                }
            }

            return false;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad( );

            CurrentOrientation = UIDevice.CurrentDevice.Orientation;

            // Instantiate all activities
            Elements.Add( new SpringboardElement( this, new NewsTask( "NewsStoryboard_iPhone" )  , NewsButton       , "watch.png" ) );
            Elements.Add( new SpringboardElement( this, new NotesTask( "" )                      , EpisodesButton   , "notes.png" ) );
            Elements.Add( new SpringboardElement( this, new GiveTask( "GiveStoryboard_iPhone" )  , GroupFinderButton, "groupfinder.png" ) );
            Elements.Add( new SpringboardElement( this, new GiveTask( "GiveStoryboard_iPhone" )  , PrayerButton     , "prayer.png" ) );
            Elements.Add( new SpringboardElement( this, new AboutTask( "AboutStoryboard_iPhone" ), AboutButton      , "info.png" ) );

            // set the profile image mask so it's circular
            CALayer maskLayer = new CALayer();
            maskLayer.AnchorPoint = new PointF( 0, 0 );
            maskLayer.Bounds = LoginButton.Layer.Bounds;
            maskLayer.CornerRadius = LoginButton.Layer.Bounds.Width / 2;
            maskLayer.BackgroundColor = UIColor.Black.CGColor;
            LoginButton.Layer.Mask = maskLayer;
            //

            AddChildViewController( NavViewController );
            View.AddSubview( NavViewController.View );

            SetNeedsStatusBarAppearanceUpdate( );
        }

        public void LoginWantsResign( )
        {
            ActiveLoginController.DismissViewController( true, null );
            ActiveLoginController = null;
        }

        public override void PrepareForSegue ( UIStoryboardSegue segue,  NSObject sender )
        {
            base.PrepareForSegue (segue, sender);

            // give the login controller a pointer to use so we can resign it.
            ActiveLoginController = segue.DestinationViewController as LoginViewController;

            if (ActiveLoginController != null) 
            {
                ActiveLoginController.Springboard = this;
            }
        }

        public override bool PrefersStatusBarHidden()
        {
            // don't show the status bar when running this app.
            return true;
        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            // only needed when we were showing the status bar. Causes
            // the status bar text to be white.
            return UIStatusBarStyle.LightContent;
        }

        protected void ActivateElement( SpringboardElement activeElement )
        {
            // don't allow any navigation while the login controller is active
            if( ActiveLoginController == null )
            {
                // first turn "off" the backingView selection for all but the element
                // becoming active.
                foreach( SpringboardElement element in Elements )
                {
                    if( element != activeElement )
                    {
                        element.BackingView.BackgroundColor = UIColor.Clear;
                    }
                }

                // activate the element and its associated task
                activeElement.BackingView.BackgroundColor = Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Config.Springboard.Element_SelectedColor );
                NavViewController.ActivateTask( activeElement.Task );
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            // don't allow any navigation while the login controller is active
            if( ActiveLoginController == null )
            {
                NavViewController.RevealSpringboard( false );
            }
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

            // if we're appearing and no task is active, start one.
            // (this will only happen when the app is first launched)
            if( NavViewController.CurrentTask == null )
            {
                ActivateElement( Elements[0] );
            }

            // are we logged in?
            if( MobileUser.Instance.LoggedIn )
            {
                // get their profile
                UserNameField.Text = MobileUser.Instance.Person.FirstName + " " + MobileUser.Instance.Person.LastName;
            }
            else
            {
                UserNameField.Text = "Login to enable additional features.";
            }
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
