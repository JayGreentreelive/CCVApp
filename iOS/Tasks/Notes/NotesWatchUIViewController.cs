using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using MonoTouch.MediaPlayer;
using System.Drawing;
using System.Collections.Generic;
using CCVApp.Shared.Strings;

namespace iOS
{
	partial class NotesWatchUIViewController : TaskUIViewController
	{
        public string WatchUrl { get; set; }

        MPMoviePlayerController MoviePlayer  { get; set; }
        UIActivityIndicatorView ActivityIndicator { get; set; }

        bool DidDisplayError { get; set; }

        List<NSObject> ObserverHandles { get; set; }
        bool EnteringFullscreen { get; set; }
        bool ExitingFullscreen { get; set; }

		public NotesWatchUIViewController (IntPtr handle) : base (handle)
		{
            ObserverHandles = new List<NSObject>( );
		}

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if ( ExitingFullscreen == false )
            {
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

                View.AddSubview( ActivityIndicator );


                // setup a notification so we know when to hide the spinner
                NSObject handle = NSNotificationCenter.DefaultCenter.AddObserver( "MPMoviePlayerContentPreloadDidFinishNotification", ContentPreloadDidFinish );
                ObserverHandles.Add( handle );

                // setup a notification so we know when they enter fullscreen, cause we'll need to play the movie again
                handle = NSNotificationCenter.DefaultCenter.AddObserver( "MPMoviePlayerPlaybackStateDidChangeNotification", PlaybackStateDidChange );
                ObserverHandles.Add( handle );

                handle = NSNotificationCenter.DefaultCenter.AddObserver( "MPMoviePlayerPlaybackDidFinishNotification", PlaybackDidFinish );
                ObserverHandles.Add( handle );


                // monitor our fullscreen status so we can manage a flag and ignore ViewDidAppear/ViewDidDisappear
                handle = NSNotificationCenter.DefaultCenter.AddObserver( "MPMoviePlayerWillEnterFullscreenNotification", WillEnterFullscreen );
                ObserverHandles.Add( handle );

                handle = NSNotificationCenter.DefaultCenter.AddObserver( "MPMoviePlayerDidEnterFullscreenNotification", DidEnterFullscreen );
                ObserverHandles.Add( handle );

                handle = NSNotificationCenter.DefaultCenter.AddObserver( "MPMoviePlayerWillExitFullscreenNotification", WillExitFullscreen );
                ObserverHandles.Add( handle );

                handle = NSNotificationCenter.DefaultCenter.AddObserver( "MPMoviePlayerDidExitFullscreenNotification", DidExitFullscreen );
                ObserverHandles.Add( handle );
            }
            else
            {
                ActivityIndicator.RemoveFromSuperview( );
            }
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

            // don't do anything if we're simply exiting fullscreen
            if ( ExitingFullscreen == false )
            {
                // when the view appears start the movie by default
                ActivityIndicator.Hidden = false;

                DidDisplayError = false;

                MoviePlayer.Play( );
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            // only process this if we're not entering fullscreen
            if ( EnteringFullscreen == false )
            {
                MoviePlayer.Stop( );

                foreach ( NSObject handle in ObserverHandles )
                {
                    NSNotificationCenter.DefaultCenter.RemoveObserver( handle );
                }

                ObserverHandles.Clear( );
            }
        }

        public void ShareVideo( )
        {
            string noteString = MessagesStrings.Watch_Share_Header_Html + string.Format( MessagesStrings.Watch_Share_Body_Html, WatchUrl );

            // if they set a mobile app url, add that.
            if( string.IsNullOrEmpty( MessagesStrings.Watch_Mobile_App_Url ) == false )
            {
                noteString += string.Format( MessagesStrings.Watch_Share_DownloadApp_Html, MessagesStrings.Watch_Mobile_App_Url );
            }

            var items = new NSObject[] { new NSString( noteString ) };

            UIActivityViewController shareController = new UIActivityViewController( items, null );
            shareController.SetValueForKey( new NSString( MessagesStrings.Watch_Share_Subject ), new NSString( "subject" ) );

            shareController.ExcludedActivityTypes = new NSString[] { UIActivityType.PostToFacebook, 
                UIActivityType.AirDrop, 
                UIActivityType.PostToTwitter, 
                UIActivityType.CopyToPasteboard, 
                UIActivityType.Message };

            PresentViewController( shareController, true, null );
        }

        void ContentPreloadDidFinish( NSNotification obj )
        {
            // once the movie is ready, hide the spinner
            ActivityIndicator.Hidden = true;
        }

        void WillEnterFullscreen( NSNotification obj )
        {
            EnteringFullscreen = true;
        }

        void DidEnterFullscreen( NSNotification obj )
        {
            EnteringFullscreen = false;
        }

        void WillExitFullscreen( NSNotification obj )
        {
            ExitingFullscreen = true;
        }

        void DidExitFullscreen( NSNotification obj )
        {
            ExitingFullscreen = false;
        }

        void PlaybackStateDidChange( NSNotification obj )
        {
            DidDisplayError = false;
        }

        void PlaybackDidFinish( NSNotification obj )
        {
            // watch for any playback errors. This would include failing to play the video in the first place.
            int error = (obj.UserInfo[ "MPMoviePlayerPlaybackDidFinishReasonUserInfoKey"] as NSNumber).IntValue;

            // if there WAS an error, report it to the user. Watch our error flag so we don't show the error
            // more than once
            if( (int)MPMovieFinishReason.PlaybackError == error && DidDisplayError == false )
            {
                DidDisplayError = true;

                SpringboardViewController.DisplayError( MessagesStrings.Error_Title, MessagesStrings.Error_Watch_Playback );
                MoviePlayer.Stop( );
                ActivityIndicator.Hidden = true;
            }
        }
	}
}
