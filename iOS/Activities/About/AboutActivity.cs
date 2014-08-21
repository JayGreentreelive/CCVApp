using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace iOS
{
    public class AboutActivity : Activity
    {
        protected UIViewController MainPageVC { get; set; }
        protected UIViewController DetailsPageVC { get; set; }
        protected UIViewController MoreDetailsPageVC { get; set; }

        protected UIViewController CurrentVC { get; set; }

        public AboutActivity( string storyboardName ) : base( storyboardName )
        {
            MainPageVC = Storyboard.InstantiateViewController( "MainPageViewController" ) as UIViewController;
            DetailsPageVC = Storyboard.InstantiateViewController( "DetailsPageViewController" ) as UIViewController;
            MoreDetailsPageVC = Storyboard.InstantiateViewController( "MoreDetailsViewController" ) as UIViewController;


        }

        public override void MakeActive( UIViewController parentViewController, NavToolbar navToolbar )
        {
            base.MakeActive( parentViewController, navToolbar );

            // for now always make the main page the starting vc
            CurrentVC = MainPageVC;

            // set our current page as root
            ((UINavigationController)parentViewController).PushViewController(CurrentVC, false);
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

            CurrentVC = null;
        }

        public override void AppOnResignActive( )
        {
            base.AppOnResignActive( );
        }
    }
}
