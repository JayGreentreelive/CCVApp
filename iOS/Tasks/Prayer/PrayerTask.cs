using System;
using UIKit;
using CoreGraphics;
using Foundation;

namespace iOS
{
    public class PrayerTask : Task
    {
        PrayerMainUIViewController MainPage { get; set; }
        TaskUIViewController ActiveViewController { get; set; }

        public PrayerTask( string storyboardName ) : base( storyboardName )
        {
            MainPage = Storyboard.InstantiateViewController( "PrayerMainUIViewController" ) as PrayerMainUIViewController;
            MainPage.Task = this;

            ActiveViewController = MainPage;
        }

        public override void MakeActive( TaskUINavigationController parentViewController, NavToolbar navToolbar )
        {
            base.MakeActive( parentViewController, navToolbar );

            // set our current page as root
            parentViewController.PushViewController(MainPage, false);
        }

        public override void WillShowViewController(UIViewController viewController)
        {
            ActiveViewController = (TaskUIViewController)viewController;

            // if it's the main page, disable the back button on the toolbar
            if ( viewController == MainPage )
            {
                NavToolbar.SetShareButtonEnabled( false, null );
                NavToolbar.SetCreateButtonEnabled( true, null );

                NavToolbar.Reveal( true );
            }
            else
            {
                NavToolbar.SetShareButtonEnabled( false, null );
                NavToolbar.SetCreateButtonEnabled( false, null );

                // if we're showing the post controller, don't reveal the nav bar,
                // as nothing should be allowed while posting.
                if ( viewController as Prayer_PostUIViewController == null )
                {
                    NavToolbar.RevealForTime( 3.0f );
                }
            }
        }

        public override void TouchesEnded(TaskUIViewController taskUIViewController, NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(taskUIViewController, touches, evt);

            if ( ActiveViewController != MainPage )
            {
                // if they touched a dead area, reveal the nav toolbar again.
                NavToolbar.RevealForTime( 3.0f );
            }
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

        public override void AppDidEnterBackground( )
        {
            base.AppDidEnterBackground( );

            ActiveViewController.AppDidEnterBackground( );
        }

        public override void AppWillTerminate()
        {
            base.AppWillTerminate( );

            ActiveViewController.AppWillTerminate( );
        }
    }
}

