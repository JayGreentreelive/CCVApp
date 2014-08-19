using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace iOS
{
    public class NewsActivity : Activity
    {
        UIViewController MainPageVC { get; set; }
        NavChildUIViewController DetailsPageVC { get; set; }
        UIViewController MoreDetailsPageVC { get; set; }

        public NewsActivity( string storyboardName ) : base( storyboardName )
        {
            MainPageVC = Storyboard.InstantiateViewController( "MainPageViewController" ) as UIViewController;
            DetailsPageVC = Storyboard.InstantiateViewController( "DetailsViewController" ) as NavChildUIViewController;
            MoreDetailsPageVC = Storyboard.InstantiateViewController( "MoreDetailsViewController" ) as UIViewController;
        }

        public override void MakeActive( UIViewController parentViewController )
        {
            base.MakeActive( parentViewController );

            ParentViewController.AddChildViewController( MainPageVC );
            ParentViewController.View.AddSubview( MainPageVC.View );
        }

        public override void OnResignActive( )
        {
            base.OnResignActive( );

            MainPageVC.View.RemoveFromSuperview( );
            MainPageVC.RemoveFromParentViewController( );
        }
    }
}

