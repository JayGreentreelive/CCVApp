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

            NavToolbar.Reveal( true );
        }

        public override void MakeInActive( )
        {
            base.MakeInActive( );

            NotesViewController.MakeInActive( );

            NotesViewController.View.RemoveFromSuperview( );
            NotesViewController.RemoveFromParentViewController( );
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
