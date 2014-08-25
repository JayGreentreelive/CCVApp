using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Droid
{
    [Activity( Label = "CCVApp Proto", MainLauncher = true, Icon = "@drawable/icon" )]
    public class MainActivity : Activity
    {
        protected override void OnCreate( Bundle bundle )
        {
            base.OnCreate( bundle );

            // If the Android version is lower than Jellybean, use this call to hide
            // the status bar.
            if (Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.JellyBean) 
            {
                Window.SetFlags( WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen );
            }
            else
            {
                this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            }

            RockMobile.PlatformCommon.Droid.Context = this;

            // Set our view from the "main" layout resource
            SetContentView( Resource.Layout.Main );

            // turn off the action bar
            ActionBar.Hide();

            // get the active task frame and give it to the springboard
            FrameLayout layout = FindViewById<FrameLayout>(Resource.Id.activetask);

            Springboard springboard = FragmentManager.FindFragmentById(Resource.Id.springboard) as Springboard;
            springboard.SetActiveTaskFrame( layout );
        }
    }
}
