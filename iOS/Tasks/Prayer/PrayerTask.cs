﻿using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace iOS
{
    public class PrayerTask : Task
    {
        PrayerMainUIViewController MainPage { get; set; }

        public PrayerTask( string storyboardName ) : base( storyboardName )
        {
            MainPage = Storyboard.InstantiateViewController( "PrayerMainUIViewController" ) as PrayerMainUIViewController;
            MainPage.Task = this;
        }

        public override void MakeActive( UINavigationController parentViewController, NavToolbar navToolbar )
        {
            base.MakeActive( parentViewController, navToolbar );

            // set our current page as root
            parentViewController.PushViewController(MainPage, false);
        }

        public override void WillShowViewController(UIViewController viewController)
        {
            NavToolbar.DisplayShareButton( false, null );

            // if it's the main page, disable the back button on the toolbar
            if ( viewController == MainPage )
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

        public override void TouchesEnded(TaskUIViewController taskUIViewController, NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(taskUIViewController, touches, evt);

            // if they touched a dead area, reveal the nav toolbar again.
            NavToolbar.RevealForTime( 3.0f );
        }

        public override void MakeInActive( )
        {
            base.MakeInActive( );

            // clean up main
            if( MainPage.View != null )
            {
                MainPage.View.RemoveFromSuperview( );
            }

            if( MainPage.ParentViewController != null )
            {
                MainPage.RemoveFromParentViewController( );
            }
        }

        public override void AppOnResignActive( )
        {
            base.AppOnResignActive( );
        }
    }
}

