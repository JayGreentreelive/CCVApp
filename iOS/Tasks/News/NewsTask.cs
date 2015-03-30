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

        List<RockNews> News { get; set; }

        public NewsTask( string storyboardName ) : base( storyboardName )
        {
            MainPageVC = Storyboard.InstantiateViewController( "MainPageViewController" ) as NewsMainUIViewController;
            MainPageVC.Task = this;

            ActiveViewController = MainPageVC;

            News = new List<RockNews>( );
            MainPageVC.SourceRockNews = News;
        }

        public override void MakeActive( TaskUINavigationController parentViewController, NavToolbar navToolbar, CGRect containerBounds )
        {
            base.MakeActive( parentViewController, navToolbar, containerBounds );

            MainPageVC.View.Bounds = containerBounds;

            ReloadNews( );

            // set our current page as root
            parentViewController.PushViewController(MainPageVC, false);
        }

        /// <summary>
        /// Takes the news from LaunchData and populates the NewsPrimaryFragment with it.
        /// </summary>
        void ReloadNews( )
        {
            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                {
                    Rock.Client.Campus campus = RockGeneralData.Instance.Data.CampusFromId( RockMobileUser.Instance.ViewingCampus );
                    Guid viewingCampusGuid = campus != null ? campus.Guid : Guid.Empty;

                    // provide the news to the viewer by COPYING it.
                    News.Clear( );
                    foreach ( RockNews newsItem in RockLaunchData.Instance.Data.News )
                    {
                        // only add news for "all campuses" and their selected campus.
                        if ( newsItem.CampusGuid == Guid.Empty || newsItem.CampusGuid == viewingCampusGuid )
                        {
                            News.Add( new RockNews( newsItem ) );
                        }
                    }
                } );
        }

        public override void WillShowViewController(TaskUIViewController viewController)
        {
            base.WillShowViewController( viewController );

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
                case "News.Reload":
                {
                    ReloadNews( );

                    MainPageVC.ReloadNews( );

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

        public override void OnActivated( )
        {
            base.OnActivated( );

            ActiveViewController.OnActivated( );
        }

        public override void WillEnterForeground()
        {
            base.WillEnterForeground();

            ActiveViewController.WillEnterForeground( );
        }

        public override void AppOnResignActive()
        {
            base.AppOnResignActive( );

            ActiveViewController.AppOnResignActive( );
        }

        public override void AppDidEnterBackground()
        {
            base.AppDidEnterBackground();

            ActiveViewController.AppDidEnterBackground( );
        }

        public override void AppWillTerminate()
        {
            base.AppWillTerminate( );

            ActiveViewController.AppWillTerminate( );
        }
    }
}
