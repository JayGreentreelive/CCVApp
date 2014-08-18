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

        public override void MakeActive( UIViewController parentViewController, PointF position )
        {
            base.MakeActive( parentViewController, position );

            NotesViewController.View.Layer.Position = position;

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

