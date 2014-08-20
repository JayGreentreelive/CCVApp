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
            PushTransition.AboutActivity = this;

            MainPageVC = Storyboard.InstantiateViewController( "MainPageViewController" ) as UIViewController;
            DetailsPageVC = Storyboard.InstantiateViewController( "DetailsPageViewController" ) as UIViewController;
            MoreDetailsPageVC = Storyboard.InstantiateViewController( "MoreDetailsViewController" ) as UIViewController;
        }

        public override void MakeActive( UIViewController parentViewController )
        {
            base.MakeActive( parentViewController );

            // for now always make the main page the starting vc
            CurrentVC = MainPageVC;

            // set our current page as root
            ((UINavigationController)parentViewController).PushViewController(CurrentVC, false);
            ((UINavigationController)parentViewController).NavigationBarHidden = true;
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

        public void PushViewController( UIViewController destinationViewController )
        {
            if( destinationViewController != MainPageVC )
            {
                ((UINavigationController)ParentViewController).NavigationBarHidden = false;
            }
            else
            {
                ((UINavigationController)ParentViewController).NavigationBarHidden = true;
            }

            ((UINavigationController)ParentViewController).PushViewController( destinationViewController, true );

            CurrentVC = destinationViewController;
        }

        public override void AppOnResignActive( )
        {
            base.AppOnResignActive( );
        }
    }

    [Register("PushTransition")]
    public class PushTransition : UIStoryboardSegue
    {
        public static AboutActivity AboutActivity { get; set; }

        public PushTransition (IntPtr param) : base (param)
        {
        }

        public override void Perform()
        {
            AboutActivity.PushViewController( DestinationViewController );
        }
    }
}
