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

            RockMobile.PlatformCommon.Droid.Context = this;

            // Set our view from the "main" layout resource
            SetContentView( Resource.Layout.Main );

            ActionBar.Hide();
        }
    }
}


