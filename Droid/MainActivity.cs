using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using Android.Gms.Maps;
using Com.Localytics.Android;

namespace Droid
{
    [Activity( Label = "CCV Mobile 2", NoHistory = true, MainLauncher = true, Icon = "@drawable/icon", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize )]
    public class Splash : Activity
    {
        protected override void OnCreate( Bundle bundle )
        {
            base.OnCreate( bundle );

            Window.AddFlags( WindowManagerFlags.Fullscreen );
            RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;

            // Set our view from the "main" layout resource
            SetContentView( Resource.Layout.Splash );

            System.Timers.Timer splashTimer = new System.Timers.Timer();
            splashTimer.Interval = 500;
            splashTimer.AutoReset = false;
            splashTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => 
                {

                    RunOnUiThread( delegate
                        {
                            // launch create order intent, which should be a FORM
                            Intent intent = new Intent(this, typeof(MainActivity));
                            StartActivity(intent);
                        });
                };

            splashTimer.Start( );
        }
    }

    [Activity( Label = "CCV Mobile 2", Icon = "@drawable/icon", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize )]
    public class MainActivity : Activity
    {
        Springboard Springboard { get; set; }

        protected override void OnCreate( Bundle bundle )
        {
            base.OnCreate( bundle );

            LocalyticsActivityLifecycleCallbacks callback = new LocalyticsActivityLifecycleCallbacks( this );
            Application.RegisterActivityLifecycleCallbacks( callback );

            Window.AddFlags(WindowManagerFlags.Fullscreen);

            // default our app to protrait mode, and let the notes change it.
            RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;

            Rock.Mobile.PlatformSpecific.Android.Core.Context = this;

            DisplayMetrics metrics = Resources.DisplayMetrics;
            Console.WriteLine("Android Device detected dpi: {0}", metrics.DensityDpi );

            // Set our view from the "main" layout resource
            SetContentView( Resource.Layout.Main );

            // get the active task frame and give it to the springboard
            FrameLayout layout = FindViewById<FrameLayout>(Resource.Id.activetask);

            Rock.Mobile.PlatformUI.PlatformBaseUI.Init( );
            MapsInitializer.Initialize( this );

            Springboard = FragmentManager.FindFragmentById(Resource.Id.springboard) as Springboard;
            Springboard.SetActiveTaskFrame( layout );
        }

        protected override void OnResume()
        {
            base.OnResume();

            OverridePendingTransition( Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out );
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            Intent = intent;
        }

        public override void OnBackPressed()
        {
            // only allow Back if the springboard OKs it.
            if ( Springboard.CanPressBack( ) )
            {
                base.OnBackPressed( );
            }
        }
    }
}
