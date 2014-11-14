
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

namespace Droid
{
    namespace Tasks
    {
        namespace Give
        {
            public class GivePrimaryFragment : TaskFragment, Android.Media.MediaPlayer.IOnPreparedListener
            {
                VideoView VideoPlayer { get; set; }

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

                    View view = inflater.Inflate(Resource.Layout.Give_Primary, container, false);
                    view.SetOnTouchListener( this );

                    VideoPlayer = new VideoView( Activity );
                    VideoPlayer.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );

                    ( ( view as RelativeLayout ) ).AddView( VideoPlayer );

                    //"http://player.vimeo.com/external/111342583.mobile.mp4?s=15dd6de0dac32e720a0955715759ea87"
                    VideoPlayer.SetVideoURI( Android.Net.Uri.Parse( "http://player.vimeo.com/external/111342583.m3u8?p=high,standard,mobile&s=f3581002f4368776af4cd07e4fc2edcf" ) );

                    VideoPlayer.SetOnPreparedListener( this );

                    // set the text to the version and build time
                    //TextView giveText = view.FindViewById<TextView>(Resource.Id.about_PrimaryFragmentText);
                    //aboutText.Text = string.Format( "CCV App Version {0}\nBuilt on {1}", CCVApp.Shared.Strings.Build.Version, CCVApp.Shared.Strings.Build.BuildTime );

                    return view;
                }

                public void OnPrepared( MediaPlayer mp )
                {
                    VideoPlayer.Start( );
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( true );

                    //VideoPlayer.Start( );
                }
            }
        }
    }
}

