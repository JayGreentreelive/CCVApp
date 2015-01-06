using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;

namespace Droid
{
    [Activity( Label = "CCV Mobile 2", MainLauncher = true, Icon = "@drawable/icon", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize )]
    public class MainActivity : Activity
    {
        protected override void OnCreate( Bundle bundle )
        {
            base.OnCreate( bundle );

            Window.AddFlags(WindowManagerFlags.Fullscreen);

            // default our app to protrait mode, and let the notes change it.
            RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;

            Rock.Mobile.PlatformSpecific.Android.Core.Context = this;

            DisplayMetrics metrics = Resources.DisplayMetrics;
            Console.WriteLine("Android Device detected dpi: {0}", metrics.DensityDpi );

            // Set our view from the "main" layout resource
            SetContentView( Resource.Layout.Main );

            // turn off the action bar
            ActionBar.Hide();

            // get the active task frame and give it to the springboard
            FrameLayout layout = FindViewById<FrameLayout>(Resource.Id.activetask);

            Rock.Mobile.PlatformUI.PlatformBaseUI.Init( );

            Springboard springboard = FragmentManager.FindFragmentById(Resource.Id.springboard) as Springboard;
            springboard.SetActiveTaskFrame( layout );
        }
    }
}
