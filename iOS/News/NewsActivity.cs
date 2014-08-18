using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace iOS
{
    public class NewsActivity : Activity
    {
        UIViewController MainPageVC { get; set; }

        public NewsActivity( string storyboardName ) : base( storyboardName )
        {
            MainPageVC = Storyboard.InstantiateViewController( "MainPageViewController" ) as UIViewController;
        }

        public override void Present( UIViewController parentViewController, PointF position )
        {
            base.Present( parentViewController, position );

            MainPageVC.View.Layer.Position = position;

            parentViewController.AddChildViewController( MainPageVC );
            parentViewController.View.AddSubview( MainPageVC.View );
        }

        public override void OnResignActive( )
        {
            base.OnResignActive( );

            MainPageVC.View.RemoveFromSuperview( );
            MainPageVC.RemoveFromParentViewController( );
        }
    }
}

