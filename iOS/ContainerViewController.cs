using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using System.Drawing;
using System.Collections.Generic;
using System.Threading;

namespace iOS
{
    /// <summary>
    /// A delegate managing the navBar owned by ContainerViewController.
    /// ContainerViewController needs to know when a user changes controllers via the navBar.
    /// </summary>
    public class NavBarDelegate : UINavigationControllerDelegate
    {
        public ContainerViewController ParentController { get; set; }
       
        public override void WillShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
        {
            // notify our parent
            ParentController.NavWillShowViewController( viewController );
        }

        public override void DidShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
        {
            // notify our parent
            ParentController.NavDidShowViewController( viewController );
        }
    }

    /// <summary>
    /// The "frame" surrounding all activities. This manages the main navigation bar
    /// that contains the Springboard Reveal button so that it can be in one place rather
    /// than making every single view controller do it.
    /// </summary>
	public partial class ContainerViewController : UIViewController
	{
        /// <summary>
        /// The activity currently being displayed.
        /// </summary>
        Activity _CurrentActivity;
        public Activity CurrentActivity { get { return _CurrentActivity; } }

        /// <summary>
        /// Each activity is placed as a child within this SubNavigation controller.
        /// Instead of using a NavigationBar to go back, however, they use the SubNavToolbar.
        /// </summary>
        /// <value>The sub navigation controller.</value>
        public UINavigationController SubNavigationController { get; set; }
        public NavToolbar SubNavToolbar { get; set; }

        /// <summary>
        /// True when the controller within an activity is animating (from a navigate forward/backward)
        /// This is tracked so we don't allow multiple navigation requests at once (like if a user spammed the back button)
        /// </summary>
        /// <value><c>true</c> if activity controller animating; otherwise, <c>false</c>.</value>
        bool ActivityControllerAnimating { get; set; }

		public ContainerViewController (IntPtr handle) : base (handle)
		{
            ActivityTransition.ContainerViewController = this;
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // container view must have a black background so that the ticks
            // before the activity displays don't cause a flash
            View.BackgroundColor = UIColor.Black;

            // First setup the SpringboardReveal button, which rests in the upper left
            // of the MainNavigationUI. (We must do it here because the ContainerViewController's
            // NavBar is the active one.)
            string imagePath = NSBundle.MainBundle.BundlePath + "/" + CCVApp.Config.PrimaryNavBar.SpringboardRevealFile;
            UIImage springboardRevealImage = new UIImage( imagePath );

            UIButton springboardRevealButton = new UIButton(UIButtonType.Custom);
            springboardRevealButton.SetImage( springboardRevealImage, UIControlState.Normal );
            springboardRevealButton.Bounds = new RectangleF( 0, 0, springboardRevealImage.Size.Width, springboardRevealImage.Size.Height );
            springboardRevealButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    (ParentViewController as MainUINavigationController).SpringboardRevealButtonTouchUp( );
                };
            this.NavigationItem.SetLeftBarButtonItem( new UIBarButtonItem( springboardRevealButton ), false );
            //


            // Now create the sub-navigation, which includes
            // the NavToolbar used to let the user navigate
            CreateSubNavigationController( );
        }

        protected void CreateSubNavigationController( )
        {
            // Load the navigationController from the storyboard.
            SubNavigationController = Storyboard.InstantiateViewController( "SubNavController" ) as UINavigationController;
            SubNavigationController.Delegate = new NavBarDelegate( ) { ParentController = this };

            // the sub navigation control should go below the primary navigation bar.
            SubNavigationController.View.Frame = new RectangleF( 0, 
                NavigationController.NavigationBar.Frame.Height, 
                SubNavigationController.View.Frame.Width, 
                SubNavigationController.View.Frame.Height - NavigationController.NavigationBar.Frame.Height);


            // setup the toolbar that will manage activity navigation and any other tasks the activity needs
            SubNavToolbar = new NavToolbar();
            SubNavToolbar.Frame = new RectangleF( 0, SubNavigationController.View.Frame.Height, View.Frame.Width, CCVApp.Config.SubNavToolbar.Height);
            SubNavToolbar.BarTintColor = RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Config.SubNavToolbar.BackgroundColor );
            SubNavToolbar.Layer.Opacity = CCVApp.Config.SubNavToolbar.Opacity;
            SubNavigationController.View.AddSubview( SubNavToolbar );


            // create the back button and place it in the nav bar
            NSString buttonLabel = new NSString(CCVApp.Config.SubNavToolbar.BackButton_Text);

            UIButton backButton = new UIButton(UIButtonType.System);
            backButton.Font = UIFont.SystemFontOfSize( CCVApp.Config.SubNavToolbar.BackButton_Size );
            backButton.SetTitle( buttonLabel.ToString( ), UIControlState.Normal );

            // determine its dimensions
            SizeF buttonSize = buttonLabel.StringSize( backButton.Font );
            backButton.Bounds = new RectangleF( 0, 0, buttonSize.Width, buttonSize.Height );

            backButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    // don't allow multiple back presses at once
                    if( ActivityControllerAnimating == false )
                    {
                        ActivityControllerAnimating = true;
                        SubNavigationController.PopViewControllerAnimated( true );
                    }
                };

            SubNavToolbar.SetBackButton( backButton );


            // add this navigation controller (and its toolbar) as a child
            // of this ContainerViewController, which will effectively make it a child
            // of the primary navigation controller.
            AddChildViewController( SubNavigationController );
            View.AddSubview( SubNavigationController.View );
        }

        public void NavWillShowViewController( UIViewController viewController )
        {
            // let the current activity know which of its view controllers was just shown.
            if( CurrentActivity != null )
            {
                CurrentActivity.WillShowViewController( viewController );
            }
        }

        public void NavDidShowViewController( UIViewController viewController )
        {
            // once the animation is COMPLETE, we can turn off the flag
            // and allow another back press.
            ActivityControllerAnimating = false;
        }

        public void ActivateActivity( Activity activity )
        {
            // reset our stack before changing activities
            SubNavigationController.PopToRootViewController( false );

            if( CurrentActivity != null )
            {
                CurrentActivity.MakeInActive( );
            }

            _CurrentActivity = activity;

            CurrentActivity.MakeActive( SubNavigationController, SubNavToolbar );
        }

        public void PerformSegue( UIViewController sourceViewController, UIViewController destinationViewController )
        {
            // notify the active activity regarding the change.
            if( CurrentActivity != null )
            {
                // take this opportunity to give the presenting view controller a pointer to the active activity
                // so it can receive callbacks.
                ActivityUIViewController viewController = destinationViewController as ActivityUIViewController;
                if( viewController == null )
                {
                    throw new InvalidCastException( "View Controllers used by Activities must be of type ActivityUIViewController" );
                }

                viewController.Activity = CurrentActivity;
                SubNavigationController.PushViewController( destinationViewController, true );
            }
        }

        public void OnResignActive()
        {
            if( CurrentActivity != null )
            {
                CurrentActivity.AppOnResignActive( );
            }
        }

        public void DidEnterBackground( )
        {
            if( CurrentActivity != null )
            {
                CurrentActivity.AppDidEnterBackground( );
            }
        }

        public void WillTerminate( )
        {
            if( CurrentActivity != null )
            {
                CurrentActivity.AppWillTerminate( );
            }
        }
	}

    // Define our activity transition class that notifies the container
    // about the transition, so it can ensure the next view controller receives
    // a reference to the active activity.
    [Register("ActivityTransition")]
    class ActivityTransition : UIStoryboardSegue
    {
        public static ContainerViewController ContainerViewController { get; set; }

        public ActivityTransition( IntPtr handle ) : base( handle )
        {
        }

        public override void Perform()
        {
            ContainerViewController.PerformSegue( SourceViewController, DestinationViewController );
        }

    }
}
