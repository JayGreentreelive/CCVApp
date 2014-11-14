
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
using Android.Webkit;

namespace Droid
{
    namespace Tasks
    {
        namespace About
        {
            public class AboutPrimaryFragment : TaskFragment
            {
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

                    View view = inflater.Inflate(Resource.Layout.About_Primary, container, false);
                    view.SetOnTouchListener( this );

                    // set the text to the version and build time
                    TextView aboutText = view.FindViewById<TextView>(Resource.Id.about_PrimaryFragmentText);
                    aboutText.Text = string.Format( "CCV App Version {0}\nBuilt on {1}", CCVApp.Shared.Strings.Build.Version, CCVApp.Shared.Strings.Build.BuildTime );

                    WebView webView = view.FindViewById<WebView>( Resource.Id.about_PrimaryFragmentWebView );

                    Activity.RunOnUiThread( delegate
                        {
                            webView.LoadUrl( CCVApp.Shared.Config.About.Url );
                        } );
                       
                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );
                }
            }
        }
    }
}

