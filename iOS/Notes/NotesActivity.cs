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
        }

        public override void OnResignActive( )
        {
            base.OnResignActive( );

            NotesViewController.View.RemoveFromSuperview( );
            NotesViewController.RemoveFromParentViewController( );
        }
    }
}

