using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace iOS
{
    public class NewsActivity : Activity
    {
        UIViewController MainPageVC { get; set; }
        UIViewController DetailsPageVC { get; set; }
        UIViewController MoreDetailsPageVC { get; set; }

        public NewsActivity( string storyboardName ) : base( storyboardName )
        {
            MainPageVC = Storyboard.InstantiateViewController( "MainPageViewController" ) as UIViewController;
            DetailsPageVC = Storyboard.InstantiateViewController( "DetailsViewController" ) as UIViewController;
            MoreDetailsPageVC = Storyboard.InstantiateViewController( "MoreDetailsViewController" ) as UIViewController;
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

            // clean up details
            if(DetailsPageVC.View != null )
            {
                DetailsPageVC.View.RemoveFromSuperview( );
            }
            if( DetailsPageVC.ParentViewController != null )
            {
                DetailsPageVC.RemoveFromParentViewController( );
            }

            // cleanup more details
            if( MoreDetailsPageVC.View != null )
            {
                MoreDetailsPageVC.View.RemoveFromSuperview( );
            }
            if( MoreDetailsPageVC.ParentViewController != null )
            {
                MoreDetailsPageVC.RemoveFromParentViewController( );
            }
        }

        public override void AppOnResignActive( )
        {
            base.AppOnResignActive( );
        }
    }
}

