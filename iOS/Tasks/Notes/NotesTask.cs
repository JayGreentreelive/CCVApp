﻿using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using CCVApp.Shared.Config;

namespace iOS
{
    public class NotesTask : Task
    {
        UIViewController ActiveViewController { get; set; }
        NotesMainUIViewController MainViewController { get; set; }

        public NotesTask( string storyboardName ) : base( storyboardName )
        {
            MainViewController = Storyboard.InstantiateViewController( "MainPageViewController" ) as NotesMainUIViewController;
            MainViewController.Task = this;
        }

        public override void MakeActive( UINavigationController parentViewController, NavToolbar navToolbar )
        {
            base.MakeActive( parentViewController, navToolbar );

            parentViewController.PushViewController( MainViewController, false );
        }

        public override void MakeInActive( )
        {
            base.MakeInActive( );

            MainViewController.View.RemoveFromSuperview( );
            MainViewController.RemoveFromParentViewController( );
        }

        public override void WillShowViewController(UIViewController viewController)
        {
            ActiveViewController = viewController;

            // if the notes are active, make sure the share button gets turned on
            if ( (viewController as NotesViewController) != null )
            {
                NavToolbar.SetBackButtonEnabled( true );
                NavToolbar.SetCreateButtonEnabled( false, null );
                NavToolbar.SetShareButtonEnabled( true, delegate
                    { 
                        (viewController as NotesViewController).ShareNotes( );
                    } );


                // go ahead and show the bar, because we're at the top of the page.
                NavToolbar.Reveal( true );
            }
            else if ( (viewController as NotesWatchUIViewController) != null )
            {
                NavToolbar.SetBackButtonEnabled( true );
                NavToolbar.SetCreateButtonEnabled( false, null );
                NavToolbar.SetShareButtonEnabled( true, delegate
                    { 
                        (viewController as NotesWatchUIViewController).ShareVideo( );
                    } );


                // go ahead and show the bar, because we're at the top of the page.
                NavToolbar.RevealForTime( 3.0f );
            }
            else
            {
                NavToolbar.SetCreateButtonEnabled( false, null );
                NavToolbar.SetShareButtonEnabled( false, null );
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

        public override void ViewDidScroll( float scrollDelta )
        {
            // did the user's finger go "up"?
            if( scrollDelta >= NoteConfig.ScrollRateForNavBarHide )
            {
                // hide the nav bar
                NavToolbar.Reveal( false );
            }
            // did the user scroll "down"?
            else if ( scrollDelta <= NoteConfig.ScrollRateForNavBarReveal )
            {
                NavToolbar.Reveal( true );
            }
        }

        public override bool CanContainerPan( NSSet touches, UIEvent evt )
        {
            NotesViewController notesVC = ActiveViewController as NotesViewController;
            if ( notesVC != null )
            {
                //return the inverse of touching a user note's bool.
                // so false if they ARE touching a note, true if they are not.
                return !notesVC.TouchingUserNote( touches, evt );
            }

            // if the notes aren't active, then of course they can pan
            return true;
        }

        public override void AppOnResignActive()
        {
            // if the notes are active and the app is being backgrounded, let the notes know so they can save.
            NotesViewController notesVC = ActiveViewController as NotesViewController;
            if ( notesVC != null )
            {
                notesVC.ViewResigning( );
            }
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            // if we're using the watch or notes controller, allow landscape
            if ( ( ActiveViewController as NotesViewController ) != null || ( ActiveViewController as NotesWatchUIViewController ) != null )
            {
                return UIInterfaceOrientationMask.All;
            }
            else
            {
                return base.GetSupportedInterfaceOrientations( );
            }
        }

        public override void AppWillTerminate()
        {
            // if the notes are active and the app is being killed, let the notes know so they can save.
            NotesViewController notesVC = ActiveViewController as NotesViewController;
            if ( notesVC != null )
            {
                notesVC.ViewResigning( );
            }
        }
    }
}
