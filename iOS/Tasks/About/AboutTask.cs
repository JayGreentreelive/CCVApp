using System;
using UIKit;
using CoreGraphics;
using Foundation;

namespace iOS
{
    public class AboutTask : Task
    {
        protected TaskUIViewController MainPageVC { get; set; }

        public AboutTask( string storyboardName ) : base( storyboardName )
        {
            MainPageVC = Storyboard.InstantiateViewController( "MainPageViewController" ) as TaskUIViewController;
            MainPageVC.Task = this;
        }

        public override void MakeActive( TaskUINavigationController parentViewController, NavToolbar navToolbar, CGRect containerBounds )
        {
            base.MakeActive( parentViewController, navToolbar, containerBounds );

            MainPageVC.View.Bounds = containerBounds;

            // set our current page as root
            parentViewController.PushViewController(MainPageVC, false);
        }

        public override void WillShowViewController(TaskUIViewController viewController)
        {
            base.WillShowViewController( viewController );

            // turn off the share & create buttons
            NavToolbar.SetShareButtonEnabled( false, null );
            NavToolbar.SetCreateButtonEnabled( false, null );

            // if it's the main page, disable the back button on the toolbar
            if ( viewController == MainPageVC )
            {
                NavToolbar.Reveal( false );
            }
            else
            {
                NavToolbar.RevealForTime( 3.0f );
            }
        }

        public override void TouchesEnded(TaskUIViewController taskUIViewController, NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(taskUIViewController, touches, evt);

            // if they touched a dead area, reveal the nav toolbar again.
            NavToolbar.RevealForTime( 3.0f );
        }

        public override void MakeInActive( )
        {
            base.MakeInActive( );
        }

        public override void OnActivated( )
        {
            base.OnActivated( );

            MainPageVC.OnActivated( );
        }

        public override void WillEnterForeground()
        {
            base.WillEnterForeground();

            MainPageVC.WillEnterForeground( );
        }

        public override void AppOnResignActive( )
        {
            base.AppOnResignActive( );

            MainPageVC.AppOnResignActive( );
        }

        public override void AppDidEnterBackground()
        {
            base.AppDidEnterBackground();

            MainPageVC.AppDidEnterBackground( );
        }

        public override void AppWillTerminate()
        {
            base.AppWillTerminate( );

            MainPageVC.AppWillTerminate( );
        }
    }
}
