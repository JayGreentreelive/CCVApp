using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace iOS
{
    public class GroupFinderTask : Task
    {
        TaskUIViewController MainPageVC { get; set; }

        public GroupFinderTask( string storyboardName ) : base( storyboardName )
        {
            MainPageVC = Storyboard.InstantiateViewController( "MainPageViewController" ) as TaskUIViewController;
            MainPageVC.Task = this;
        }

        public override void MakeActive( UINavigationController parentViewController, NavToolbar navToolbar )
        {
            base.MakeActive( parentViewController, navToolbar );

            // set our current page as root
            parentViewController.PushViewController(MainPageVC, false);
        }

        public override void MakeInActive( )
        {
            base.MakeInActive( );

            // clean up main
            if( MainPageVC.View != null )
            {
                MainPageVC.View.RemoveFromSuperview( );
            }

            if( MainPageVC.ParentViewController != null )
            {
                MainPageVC.RemoveFromParentViewController( );
            }
        }

        public override void WillShowViewController(UIViewController viewController)
        {
            // turn off the share button
            NavToolbar.DisplayShareButton( false, null );

            // if it's the main page, disable the back button on the toolbar
            if ( viewController == MainPageVC )
            {
                NavToolbar.SetBackButtonEnabled( false );
                NavToolbar.Reveal( false );
            }
            else
            {
                NavToolbar.SetBackButtonEnabled( true );
                NavToolbar.RevealForTime( 3.0f );
            }
        }

        public override void TouchesEnded(TaskUIViewController taskUIViewController, NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(taskUIViewController, touches, evt);

            // if they touched a dead area, reveal the nav toolbar again.
            NavToolbar.RevealForTime( 3.0f );
        }
    }
}

