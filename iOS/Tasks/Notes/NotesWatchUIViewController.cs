using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using MonoTouch.MediaPlayer;
using System.Drawing;

namespace iOS
{
	partial class NotesWatchUIViewController : TaskUIViewController
	{
        public string WatchUrl { get; set; }

        MPMoviePlayerController MoviePlayer  { get; set; }
        UIActivityIndicatorView ActivityIndicator { get; set; }

		public NotesWatchUIViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // don't allow anything if there isn't a watchUrl set
            if ( WatchUrl == null )
            {
                throw new Exception( "WatchUrl must not be null!" );
            }

            // setup our activity indicator
            ActivityIndicator = new UIActivityIndicatorView();
            ActivityIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.White;
            ActivityIndicator.SizeToFit( );
            ActivityIndicator.StartAnimating( );

            // create the movie player control
            MoviePlayer = new MPMoviePlayerController( new NSUrl( WatchUrl ) );
            View.AddSubview( MoviePlayer.View );

            // setup a notification so we know when to hide the spinner
            NSNotificationCenter.DefaultCenter.AddObserver( "MPMoviePlayerContentPreloadDidFinishNotification", delegate(NSNotification obj)
                {
                    // once the movie is ready, hide the spinner
                    ActivityIndicator.Hidden = true;
                } );

            // setup a notification so we know when they enter fullscreen, cause we'll need to play the movie again
            NSNotificationCenter.DefaultCenter.AddObserver( "MPMoviePlayerDidEnterFullscreenNotification", delegate(NSNotification obj)
                {
                    MoviePlayer.Play( );
                } );

            NSNotificationCenter.DefaultCenter.AddObserver( "MPMoviePlayerPlaybackDidFinishNotification", delegate(NSNotification obj )
                {
                    // watch for any playback errors. This would include failing to play the video in the first place.
                    int error = (obj.UserInfo[ "MPMoviePlayerPlaybackDidFinishReasonUserInfoKey"] as NSNumber).IntValue;

                    // if there WAS an error, report it to the user
                    if( (int)MPMovieFinishReason.PlaybackError == error )
                    {
                        SpringboardViewController.DisplayError( CCVApp.Shared.Strings.Messages.Error_Title, CCVApp.Shared.Strings.Messages.Error_Watch_Playback );
                        MoviePlayer.Stop( );
                        ActivityIndicator.Hidden = true;
                    }
                } );

            View.AddSubview( ActivityIndicator );
        }

        public override void ViewDidLayoutSubviews( )
        {
            base.ViewDidLayoutSubviews( );

            // if the orientation is portrait, we need to limit the height to the same as the width so we don't
            // overlap the bottom nav toolbar.
            if ( UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.Portrait )
            {
                MoviePlayer.View.Frame = new System.Drawing.RectangleF( 0, ( View.Frame.Height - View.Frame.Width ) / 2, View.Frame.Width, View.Frame.Width );
            }
            else
            {
                // for landscape we don't care, just use the height sans the navbar
                if ( NavigationController != null )
                {
                    float navHeight = NavigationController.NavigationBar.Frame.Height;
                    MoviePlayer.View.Frame = new System.Drawing.RectangleF( 0, navHeight, View.Frame.Width, View.Frame.Height - navHeight );
                }
                else
                {
                    MoviePlayer.View.Frame = View.Frame;
                }
            }

            ActivityIndicator.Layer.Position = new System.Drawing.PointF( ( View.Frame.Width - ActivityIndicator.Frame.Width ) / 2, ( View.Frame.Height - ActivityIndicator.Frame.Height ) / 2 );
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // when the view appears start the movie by default
            ActivityIndicator.Hidden = false;

            MoviePlayer.Play( );
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            MoviePlayer.Stop( );
        }
	}
}
