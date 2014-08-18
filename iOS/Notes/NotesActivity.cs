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

        public override void Present( UIViewController parentViewController, PointF position )
        {
            base.Present( parentViewController, position );

            NotesViewController.View.Layer.Position = position;

            parentViewController.AddChildViewController( NotesViewController );
            parentViewController.View.AddSubview( NotesViewController.View );
        }

        public override void OnResignActive( )
        {
            base.OnResignActive( );

            NotesViewController.View.RemoveFromSuperview( );
            NotesViewController.RemoveFromParentViewController( );
        }
    }
}

