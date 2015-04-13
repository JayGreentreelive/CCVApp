
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

using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;

namespace Droid
{
    namespace Tasks
    {
        namespace News
        {
            public class NewsWebFragment : TaskFragment
            {
                WebView WebView { get; set; }
                String Url { get; set; }

                bool IsActive { get; set; }

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

                    View view = inflater.Inflate(Resource.Layout.News_Web, container, false);
                    view.SetOnTouchListener( this );

                    WebView = view.FindViewById<WebView>( Resource.Id.WebView );


                    return view;
                }

                public void DisplayUrl( string url )
                {
                    Url = url;

                    // if we're active, we can go ahead and display the url.
                    // Otherwise, OnResume will take care of it.
                    if ( IsActive == true )
                    {
                        Activity.RunOnUiThread( delegate
                            {
                                WebView.LoadUrl( url );
                            } );
                    }
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    IsActive = true;

                    WebView.LoadUrl( Url );
                }

                public override void OnPause()
                {
                    base.OnPause();

                    IsActive = false;
                }
            }
        }
    }
}

