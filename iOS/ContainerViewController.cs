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
        /// The task currently being displayed.
        /// </summary>
        Task _CurrentTask;
        public Task CurrentTask { get { return _CurrentTask; } }

        /// <summary>
        /// Each task is placed as a child within this SubNavigation controller.
        /// Instead of using a NavigationBar to go back, however, they use the SubNavToolbar.
        /// </summary>
        /// <value>The sub navigation controller.</value>
        public UINavigationController SubNavigationController { get; set; }
        public NavToolbar SubNavToolbar { get; set; }

        /// <summary>
        /// True when the controller within an task is animating (from a navigate forward/backward)
        /// This is tracked so we don't allow multiple navigation requests at once (like if a user spammed the back button)
        /// </summary>
        /// <value><c>true</c> if task controller animating; otherwise, <c>false</c>.</value>
        bool TaskControllerAnimating { get; set; }

		public ContainerViewController (IntPtr handle) : base (handle)
		{
            TaskTransition.ContainerViewController = this;
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // container view must have a black background so that the ticks
            // before the task displays don't cause a flash
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


            // setup the toolbar that will manage task navigation and any other tasks the task needs
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
                    if( TaskControllerAnimating == false )
                    {
                        TaskControllerAnimating = true;
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
            // let the current task know which of its view controllers was just shown.
            if( CurrentTask != null )
            {
                CurrentTask.WillShowViewController( viewController );
            }
        }

        public void NavDidShowViewController( UIViewController viewController )
        {
            // once the animation is COMPLETE, we can turn off the flag
            // and allow another back press.
            TaskControllerAnimating = false;
        }

        public void ActivateTask( Task task )
        {
            // reset our stack before changing activities
            SubNavigationController.PopToRootViewController( false );

            if( CurrentTask != null )
            {
                CurrentTask.MakeInActive( );
            }

            _CurrentTask = task;

            CurrentTask.MakeActive( SubNavigationController, SubNavToolbar );
        }

        public void PerformSegue( UIViewController sourceViewController, UIViewController destinationViewController )
        {
            // notify the active task regarding the change.
            if( CurrentTask != null )
            {
                // take this opportunity to give the presenting view controller a pointer to the active task
                // so it can receive callbacks.
                TaskUIViewController viewController = destinationViewController as TaskUIViewController;
                if( viewController == null )
                {
                    throw new InvalidCastException( "View Controllers used by Activities must be of type TaskUIViewController" );
                }

                viewController.Task = CurrentTask;
                SubNavigationController.PushViewController( destinationViewController, true );
            }
        }

        public void OnResignActive()
        {
            if( CurrentTask != null )
            {
                CurrentTask.AppOnResignActive( );
            }
        }

        public void DidEnterBackground( )
        {
            if( CurrentTask != null )
            {
                CurrentTask.AppDidEnterBackground( );
            }
        }

        public void WillTerminate( )
        {
            if( CurrentTask != null )
            {
                CurrentTask.AppWillTerminate( );
            }
        }
	}

    // Define our task transition class that notifies the container
    // about the transition, so it can ensure the next view controller receives
    // a reference to the active task.
    [Register("TaskTransition")]
    class TaskTransition : UIStoryboardSegue
    {
        public static ContainerViewController ContainerViewController { get; set; }

        public TaskTransition( IntPtr handle ) : base( handle )
        {
        }

        public override void Perform()
        {
            ContainerViewController.PerformSegue( SourceViewController, DestinationViewController );
        }

    }
}
