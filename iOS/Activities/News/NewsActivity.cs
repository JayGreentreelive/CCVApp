using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace iOS
{
    public class NewsActivity : Activity
    {
        NavChildUIViewController MainPageVC { get; set; }
        NavChildUIViewController DetailsPageVC { get; set; }
        NavChildUIViewController MoreDetailsPageVC { get; set; }

        public NewsActivity( string storyboardName ) : base( storyboardName )
        {
            MainPageVC = Storyboard.InstantiateViewController( "MainPageViewController" ) as NavChildUIViewController;
            DetailsPageVC = Storyboard.InstantiateViewController( "DetailsViewController" ) as NavChildUIViewController;
            MoreDetailsPageVC = Storyboard.InstantiateViewController( "MoreDetailsViewController" ) as NavChildUIViewController;
        }

        public override void MakeActive( UIViewController parentViewController )
        {
            base.MakeActive( parentViewController );

            ParentViewController.AddChildViewController( MainPageVC );
            ParentViewController.View.AddSubview( MainPageVC.View );
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

