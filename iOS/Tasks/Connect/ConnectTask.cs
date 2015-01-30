using System;
using UIKit;
using CoreGraphics;
using Foundation;

namespace iOS
{
    public class ConnectTask : Task
    {
        TaskUIViewController MainPageVC { get; set; }

        public ConnectTask( string storyboardName ) : base( storyboardName )
        {
            MainPageVC = Storyboard.InstantiateViewController( "ConnectMainPageViewController" ) as TaskUIViewController;
            MainPageVC.Task = this;
        }

        public override void MakeActive( TaskUINavigationController parentViewController, NavToolbar navToolbar )
        {
            base.MakeActive( parentViewController, navToolbar );

            // set our current page as root
            parentViewController.PushViewController(MainPageVC, false);
        }

        public override void MakeInActive( )
        {
            base.MakeInActive( );
        }

        public override void WillShowViewController(UIViewController viewController)
        {
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
                if ( viewController as ConnectWebViewController != null )
                {
                    NavToolbar.Reveal( true );
                }
                else
                {
                    NavToolbar.RevealForTime( 3.0f );
                }
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

