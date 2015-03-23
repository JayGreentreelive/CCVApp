
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
        namespace Notes
        {
            public class NotesWebViewFragment : TaskFragment
            {
                public string ActiveUrl { get; set; }

                ProgressBar ActivityIndicator { get; set; }

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

                    View view = inflater.Inflate(Resource.Layout.Notes_WebView, container, false);
                    view.SetOnTouchListener( this );

                    ActivityIndicator = view.FindViewById<ProgressBar>( Resource.Id.activityIndicator );
                    ActivityIndicator.Visibility = ViewStates.Visible;

                    WebView webView = view.FindViewById<WebView>( Resource.Id.webView );
                    webView.Settings.JavaScriptEnabled = true;
                    webView.Settings.SetSupportZoom(true);
                    webView.Settings.BuiltInZoomControls = true;
                    webView.Settings.LoadWithOverviewMode = true; //Load 100% zoomed out
                    webView.ScrollBarStyle = ScrollbarStyles.OutsideOverlay;
                    webView.ScrollbarFadingEnabled = true;

                    webView.VerticalScrollBarEnabled = true;
                    webView.HorizontalScrollBarEnabled = true;
                    webView.SetWebViewClient( new NoteWebViewClient() { Parent = this } );

                    Activity.RunOnUiThread( delegate
                        {
                            webView.LoadUrl( ActiveUrl );
                        } );
                       
                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( true );
                }

                public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
                {
                    base.OnConfigurationChanged(newConfig);

                    if( newConfig.Orientation == Android.Content.Res.Orientation.Landscape )
                    {
                        ParentTask.NavbarFragment.EnableSpringboardRevealButton( false );
                        //ParentTask.NavbarFragment.ToggleFullscreen( true );
                        //ParentTask.NavbarFragment.NavToolbar.Reveal( false );
                    }
                    else
                    {
                        ParentTask.NavbarFragment.EnableSpringboardRevealButton( true );
                        //ParentTask.NavbarFragment.ToggleFullscreen( false );
                    }
                }

                class NoteWebViewClient : WebViewClient
                {
                    public NotesWebViewFragment Parent { get; set; }

                    public override void OnPageFinished(WebView view, string url)
                    {
                        base.OnPageFinished(view, url);

                        Parent.OnPageFinished( view, url );
                    }
                }

                public void OnPageFinished( WebView view, string url )
                {
                    ActivityIndicator.Visibility = ViewStates.Gone;
                }
            }
        }
    }
}

