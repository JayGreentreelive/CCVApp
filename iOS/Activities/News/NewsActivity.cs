using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace iOS
{
    public class NewsActivity : Activity
    {
        ActivityUIViewController MainPageVC { get; set; }

        public NewsActivity( string storyboardName ) : base( storyboardName )
        {
            MainPageVC = Storyboard.InstantiateViewController( "MainPageViewController" ) as ActivityUIViewController;
            MainPageVC.Activity = this;
        }

        public override void MakeActive( UIViewController parentViewController, NavToolbar navToolbar )
        {
            base.MakeActive( parentViewController, navToolbar );

            // set our current page as root
            ((UINavigationController)parentViewController).PushViewController(MainPageVC, false);
        }

        public override void WillShowViewController(UIViewController viewController)
        {
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

        public override void TouchesEnded(ActivityUIViewController activityUIViewController, NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(activityUIViewController, touches, evt);

            // if they touched a dead area, reveal the nav toolbar again.
            NavToolbar.RevealForTime( 3.0f );
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

        public override void AppOnResignActive( )
        {
            base.AppOnResignActive( );
        }
    }
}

