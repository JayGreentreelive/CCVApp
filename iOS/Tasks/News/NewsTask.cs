using System;
using UIKit;
using CoreGraphics;
using Foundation;
using CCVApp.Shared.Network;
using System.Collections.Generic;

namespace iOS
{
    public class NewsTask : Task
    {
        NewsMainUIViewController MainPageVC { get; set; }

        public NewsTask( string storyboardName ) : base( storyboardName )
        {
            MainPageVC = Storyboard.InstantiateViewController( "MainPageViewController" ) as NewsMainUIViewController;
            MainPageVC.Task = this;
        }

        public override void MakeActive( TaskUINavigationController parentViewController, NavToolbar navToolbar )
        {
            base.MakeActive( parentViewController, navToolbar );

            // provide the news to the viewer by COPYING it.
            MainPageVC.SourceRockNews.Clear( );
            foreach ( RockNews newsItem in RockLaunchData.Instance.Data.News )
            {
                MainPageVC.SourceRockNews.Add( new RockNews( newsItem ) );
            }

            // set our current page as root
            parentViewController.PushViewController(MainPageVC, false);
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
                NavToolbar.RevealForTime( 3.0f );
            }
        }

        public override void PerformAction(string action)
        {
            base.PerformAction(action);

            switch ( action )
            {
                case "Task.Init":
                {
                    MainPageVC.DownloadImages( );
                    break;
                }
            }
        }

        public override void TouchesEnded(TaskUIViewController taskUIViewController, NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(taskUIViewController, touches, evt);

            // if they touched a dead area, reveal the nav toolbar again.
            NavToolbar.RevealForTime( 3.0f );
        }

        public override void MakeInActive( )
        {
            base.MakeInActive( );
        }

        public override void AppOnResignActive( )
        {
            base.AppOnResignActive( );
        }
    }
}
