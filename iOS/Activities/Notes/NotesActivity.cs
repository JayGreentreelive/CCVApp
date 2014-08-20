using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace iOS
{
    public class NotesActivity : Activity
    {
        NotesViewController NotesViewController { get; set; }

        public NotesActivity( string storyboardName ) : base( storyboardName )
        {
            NotesViewController = new NotesViewController( );
        }

        public override void MakeActive( UIViewController parentViewController )
        {
            base.MakeActive( parentViewController );

            ParentViewController.AddChildViewController( NotesViewController );
            ParentViewController.View.AddSubview( NotesViewController.View );

            NotesViewController.MakeActive( );
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
