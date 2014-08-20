using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace iOS
{
    public class GiveActivity : Activity
    {
        NavChildUIViewController MainPageVC { get; set; }

        public GiveActivity( string storyboardName ) : base( storyboardName )
        {
            MainPageVC = Storyboard.InstantiateViewController( "MainPageViewController" ) as NavChildUIViewController;
        }

        public override void MakeActive( UIViewController parentViewController )
        {
            base.MakeActive( parentViewController );

            ParentViewController.AddChildViewController( MainPageVC );
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

