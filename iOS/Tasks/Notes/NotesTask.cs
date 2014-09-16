using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace iOS
{
    public class NotesTask : Task
    {
        NotesViewController NotesViewController { get; set; }

        public NotesTask( string storyboardName ) : base( storyboardName )
        {
            NotesViewController = new NotesViewController( );
            NotesViewController.Task = this;
        }

        public override void MakeActive( UINavigationController parentViewController, NavToolbar navToolbar )
        {
            base.MakeActive( parentViewController, navToolbar );

            // for now, let the note name be the previous saturday
            DateTime time = DateTime.UtcNow;

            // if it's not saturday, find the date of the past saturday
            if( time.DayOfWeek != DayOfWeek.Saturday )
            {
                time = time.Subtract( new TimeSpan( (int)time.DayOfWeek + 1, 0, 0, 0 ) );
            }
            NotesViewController.NoteName = string.Format("{0}_{1}_{2}_{3}", CCVApp.Shared.Config.Note.NamePrefix, time.Month, time.Day, time.Year );
            //

            // set our current page as root
            parentViewController.PushViewController(NotesViewController, false);

            NotesViewController.MakeActive( );
        }

        public override void MakeInActive( )
        {
            base.MakeInActive( );

            NotesViewController.MakeInActive( );

            NotesViewController.View.RemoveFromSuperview( );
            NotesViewController.RemoveFromParentViewController( );
        }

        public override void WillShowViewController(UIViewController viewController)
        {
            // if it's the main page, disable the back button on the toolbar
            if ( viewController == NotesViewController )
            {
                NavToolbar.SetBackButtonEnabled( false );

                // go ahead and show the bar, because we're at the top of the page.
                NavToolbar.Reveal( true );
            }
        }

        public override void ViewDidScroll( float scrollDelta )
        {
            Console.WriteLine( "Move Rate: {0}", scrollDelta );

            // did the user's finger go "up"?
            if( scrollDelta >= CCVApp.Shared.Config.Note.ScrollRateForNavBarHide )
            {
                // hide the nav bar
                NavToolbar.Reveal( false );
            }
            // did the user scroll "down"?
            else if ( scrollDelta <= CCVApp.Shared.Config.Note.ScrollRateForNavBarReveal )
            {
                NavToolbar.Reveal( true );
            }
        }

        public override void AppOnResignActive()
        {
            NotesViewController.OnResignActive( );
        }

        public override void AppWillTerminate()
        {
            NotesViewController.WillTerminate( );
        }
    }
}
