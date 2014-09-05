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
