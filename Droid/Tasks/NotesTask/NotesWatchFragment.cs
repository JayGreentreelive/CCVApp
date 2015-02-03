﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Media;
using CCVApp.Shared.Strings;
using Rock.Mobile.PlatformUI;
using CCVApp.Shared;
using CCVApp.Shared.Analytics;

namespace Droid
{
    namespace Tasks
    {
        namespace Notes
        {
            public class NotesWatchFragment : TaskFragment, Android.Media.MediaPlayer.IOnPreparedListener, Android.Media.MediaPlayer.IOnErrorListener, Android.Media.MediaPlayer.IOnSeekCompleteListener
            {
                VideoView VideoPlayer { get; set; }
                MediaController MediaController { get; set; }
                ProgressBar ProgressBar { get; set; }

                public string VideoUrl { get; set; }
                public string ShareUrl { get; set; }
                public string Name { get; set; }

                public override void OnCreate( Bundle savedInstanceState )
                {
                    base.OnCreate( savedInstanceState );
                }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    if (container == null)
                    {
                        // Currently in a layout without a container, so no reason to create our view.
                        return null;
                    }

                    MediaController = new MediaController( Rock.Mobile.PlatformSpecific.Android.Core.Context );

                    RelativeLayout view = new RelativeLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    view.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
                    view.SetBackgroundColor( Android.Graphics.Color.Black );
                    view.SetOnTouchListener( this );

                    VideoPlayer = new VideoView( Activity );
                    VideoPlayer.SetMediaController( MediaController );
                    VideoPlayer.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    ( (RelativeLayout.LayoutParams)VideoPlayer.LayoutParameters ).AddRule( LayoutRules.CenterInParent );

                    ( ( view as RelativeLayout ) ).AddView( VideoPlayer );

                    VideoPlayer.SetOnPreparedListener( this );
                    VideoPlayer.SetOnErrorListener( this );

                    ProgressBar = new ProgressBar( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    ProgressBar.Indeterminate = true;
                    ProgressBar.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( 0 ) );
                    ProgressBar.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (RelativeLayout.LayoutParams)ProgressBar.LayoutParameters ).AddRule( LayoutRules.CenterInParent );
                    view.AddView( ProgressBar );
                    ProgressBar.BringToFront();

                    return view;
                }

                public void OnPrepared( MediaPlayer mp )
                {
                    // now that the video is ready we can hide the progress bar
                    ProgressBar.Visibility = ViewStates.Gone;

                    MediaController.SetAnchorView( VideoPlayer );

                    // setup a seek listener
                    mp.SetOnSeekCompleteListener( this );

                    // log the series they tapped on.
                    MessageAnalytic.Instance.Trigger( MessageAnalytic.Watch, Name );

                    // if this is a new video, store the URL
                    if ( CCVApp.Shared.Network.RockMobileUser.Instance.LastStreamingVideoUrl != VideoUrl )
                    {
                        CCVApp.Shared.Network.RockMobileUser.Instance.LastStreamingVideoUrl = VideoUrl;
                        VideoPlayer.Start( );
                    }
                    else
                    {
                        // otherwise, resume where we left off
                        mp.SeekTo( (int)CCVApp.Shared.Network.RockMobileUser.Instance.LastStreamingVideoPos );
                    }
                }

                public void OnSeekComplete( MediaPlayer mp )
                {
                    VideoPlayer.Start( );
                }

                public bool OnError( MediaPlayer mp, MediaError error, int extra )
                {
                    ProgressBar.Visibility = ViewStates.Gone;
                    Springboard.DisplayError( MessagesStrings.Error_Title, MessagesStrings.Error_Watch_Playback );

                    return true;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    Activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.FullSensor;

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( true );

                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( true, delegate 
                        {
                            // Generate an email advertising this video.
                            Intent sendIntent = new Intent();
                            sendIntent.SetAction( Intent.ActionSend );

                            sendIntent.PutExtra( Intent.ExtraSubject, MessagesStrings.Watch_Share_Subject );

                            string noteString = MessagesStrings.Watch_Share_Header_Html + string.Format( MessagesStrings.Watch_Share_Body_Html, ShareUrl );

                            // if they set a mobile app url, add that.
                            if( string.IsNullOrEmpty( MessagesStrings.Watch_Mobile_App_Url ) == false )
                            {
                                noteString += string.Format( MessagesStrings.Watch_Share_DownloadApp_Html, MessagesStrings.Watch_Mobile_App_Url );
                            }

                            sendIntent.PutExtra( Intent.ExtraText, Android.Text.Html.FromHtml( noteString ) );
                            sendIntent.SetType( "text/html" );
                            StartActivity( sendIntent );
                        });

                    if ( string.IsNullOrEmpty( VideoUrl ) )
                    {
                        throw new Exception( "VideoUrl must not be null." );
                    }

                    ProgressBar.Visibility = ViewStates.Visible;

                    VideoPlayer.SetVideoURI( Android.Net.Uri.Parse( VideoUrl ) );
                    VideoPlayer.Pause( );
                }

                public override void OnPause()
                {
                    base.OnPause();

                    ParentTask.NavbarFragment.EnableSpringboardRevealButton( true );
                    ParentTask.NavbarFragment.ToggleFullscreen( false );

                    // see if we should store the playback position for resuming
                    if ( VideoPlayer.Duration > 0 )
                    {
                        // if we're within 10 and 90 percent, do it
                        float playbackPerc = (float)VideoPlayer.CurrentPosition / (float)VideoPlayer.Duration;
                        if ( playbackPerc > .10f && playbackPerc < .95f )
                        {
                            CCVApp.Shared.Network.RockMobileUser.Instance.LastStreamingVideoPos = VideoPlayer.CurrentPosition;
                        }
                        else
                        {
                            // otherwise plan on starting from the beginning
                            CCVApp.Shared.Network.RockMobileUser.Instance.LastStreamingVideoPos = 0;
                        }
                    }


                    // stop playback
                    VideoPlayer.StopPlayback( );
                }

                public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
                {
                    base.OnConfigurationChanged(newConfig);

                    if( newConfig.Orientation == Android.Content.Res.Orientation.Landscape )
                    {
                        ParentTask.NavbarFragment.EnableSpringboardRevealButton( false );
                        ParentTask.NavbarFragment.ToggleFullscreen( true );
                        ParentTask.NavbarFragment.NavToolbar.Reveal( false );
                    }
                    else
                    {
                        ParentTask.NavbarFragment.EnableSpringboardRevealButton( true );
                        ParentTask.NavbarFragment.ToggleFullscreen( false );
                    }
                }
            }
        }
    }
}

