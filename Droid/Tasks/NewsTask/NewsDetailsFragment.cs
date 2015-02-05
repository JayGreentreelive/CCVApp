
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
                    view.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    // set the banner
                    Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView banner = new Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView( Activity );
                    ( (LinearLayout)view ).AddView( banner, 0 );

                    // IS there a banner?
                    Bitmap imageBanner = null;
                    if ( string.IsNullOrEmpty( NewsItem.HeaderImageName ) == false )
                    {
                        System.IO.Stream assetStream = Activity.BaseContext.Assets.Open( NewsItem.HeaderImageName );
                        imageBanner = BitmapFactory.DecodeStream( assetStream );
                    }
                    else
                    {
                        // if not, use the placeholder
                        imageBanner = BitmapFactory.DecodeResource( Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources, Resource.Drawable.thumbnailPlaceholder );
                    }
                    banner.SetImageBitmap( imageBanner );

                    TextView title = view.FindViewById<TextView>( Resource.Id.news_details_title );
                    title.Text = NewsItem.Title;
                    title.SetSingleLine( );
                    title.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
                    title.SetMaxLines( 1 );
                    title.SetHorizontallyScrolling( true );
                    ControlStyling.StyleUILabel( title, ControlStylingConfig.Large_Font_Bold, ControlStylingConfig.Large_FontSize );

                    // set the description
                    TextView description = view.FindViewById<TextView>( Resource.Id.news_details_details );
                    description.Text = NewsItem.Description;
                    description.MovementMethod = new ScrollingMovementMethod();
                    //description.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                    ControlStyling.StyleUILabel( description, ControlStylingConfig.Small_Font_Light, ControlStylingConfig.Small_FontSize );

                    Button launchUrlButton = view.FindViewById<Button>(Resource.Id.news_details_launch_url);
                    launchUrlButton.Click += (object sender, EventArgs e) => 
                        {
                            // move to the next page..somehow.
                            ParentTask.OnClick( this, launchUrlButton.Id );
                        };
                    ControlStyling.StyleButton( launchUrlButton, NewsStrings.LearnMore, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );

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
