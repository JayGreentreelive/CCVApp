using System;
using Rock.Mobile.PlatformUI;
using System.Drawing;

namespace CCVApp.Shared.UI
{
    /// <summary>
    /// Used to display a result to a user, including a status message
    /// </summary>
    public class UIBlockerView
    {
        PlatformView View { get; set; }
        PlatformBusyIndicator BusyIndicator { get; set; }

        public UIBlockerView( object parentView, RectangleF bounds )
        {
            // setup the fullscreen blocker view
            View = PlatformView.Create( );
            View.AddAsSubview( parentView );
            View.UserInteractionEnabled = false;
            View.BackgroundColor = 0x000000FF;

            // and the busy indicator
            BusyIndicator = PlatformBusyIndicator.Create( );
            BusyIndicator.AddAsSubview( parentView );

            BusyIndicator.Color = 0x999999FF;
            BusyIndicator.BackgroundColor = 0;

            // default to hidden
            SetOpacity( 0.00f );
        }

        void SetOpacity( float opacity )
        {
            View.Opacity = opacity;
            BusyIndicator.Opacity = opacity;
        }

        public void SetBounds( RectangleF bounds )
        {
            View.Bounds = bounds;

            float width = 100;
            float height = 100;
            BusyIndicator.Frame = new RectangleF( (bounds.Width - width) / 2, (bounds.Height - height) / 2, width, height );
        }

        public void Show( )
        {
            Util.AnimateBackgroundOpacity( View, 1.00f );
            Util.AnimateBackgroundOpacity( BusyIndicator, 1.00f );
        }

        public void Hide( )
        {
            Util.AnimateBackgroundOpacity( View, 0.00f );
            Util.AnimateBackgroundOpacity( BusyIndicator, 0.00f );
        }
    }
}

