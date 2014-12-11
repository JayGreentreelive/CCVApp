
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
using Android.Graphics;
using Rock.Mobile.PlatformCommon;
using Rock.Mobile.PlatformUI;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;
using Android.Text.Method;

namespace Droid
{
    namespace Tasks
    {
        namespace News
        {
            public class NewsDetailsFragment : TaskFragment
            {
                public CCVApp.Shared.Network.RockNews NewsItem { get; set; }

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

                    View view = inflater.Inflate(Resource.Layout.News_Details, container, false);
                    view.SetOnTouchListener( this );
                    view.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    // set the banner
                    DroidScaledImageView banner = new DroidScaledImageView( Activity );
                    ( (LinearLayout)view ).AddView( banner, 0 );

                    System.IO.Stream assetStream = Activity.BaseContext.Assets.Open( NewsItem.HeaderImageName );
                    banner.SetImageBitmap( BitmapFactory.DecodeStream( assetStream ) );


                    // set the description
                    TextView description = view.FindViewById<TextView>( Resource.Id.news_details_details );
                    description.Text = NewsItem.Description;
                    description.MovementMethod = new ScrollingMovementMethod();
                    description.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );


                    Button launchUrlButton = view.FindViewById<Button>(Resource.Id.news_details_launch_url);
                    launchUrlButton.Click += (object sender, EventArgs e) => 
                        {
                            // move to the next page..somehow.
                            ParentTask.OnClick( this, launchUrlButton.Id );
                        };
                    ControlStyling.StyleButton( launchUrlButton, NewsStrings.LearnMore );

                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.RevealForTime( 3.00f );
                }
            }
        }
    }
}
