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

            // Set our view from the "main" layout resource
            SetContentView( Resource.Layout.Main );

            // kick off our intent for the Notes activity
            Intent intent = new Intent( this, typeof( NotesActivity ) );
            StartActivity( intent );
        }
    }
}


