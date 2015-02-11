
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
using CCVApp.Shared.Network;
using Android.Graphics;
using Rock.Mobile.PlatformUI;
using System.Drawing;
using CCVApp.Shared.Strings;
using CCVApp.Shared.Config;
using CCVApp.Shared.Analytics;

namespace Droid
{
    namespace Tasks
    {
        namespace Prayer
        {
            public class SpinnerArrayAdapter : ArrayAdapter
            {
                int ResourceId { get; set; }
                public SpinnerArrayAdapter( Context context, int resourceId ) : base( context, resourceId )
                {
                    ResourceId = resourceId;
                }

                public override View GetView(int position, View convertView, ViewGroup parent)
                {
                    if ( convertView as TextView == null )
                    {
                        convertView = ( Context as Activity ).LayoutInflater.Inflate( ResourceId, parent, false );
                        ControlStyling.StyleUILabel( (convertView as TextView), ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
                    }

                    ( convertView as TextView ).Text = this.GetItem( position ).ToString( );

                    return convertView;
                }
            }

            public class PrayerCreateFragment : TaskFragment
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

                    View view = inflater.Inflate(Resource.Layout.Prayer_Create, container, false);
                    view.SetOnTouchListener( this );

                    view.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    // setup the name background
                    RelativeLayout backgroundLayout = view.FindViewById<RelativeLayout>( Resource.Id.name_background );
                    ControlStyling.StyleBGLayer( backgroundLayout );

                    View borderView = backgroundLayout.FindViewById<View>( Resource.Id.middle_border );
                    borderView.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );
                    //

                    // setup the prayer request background
                    backgroundLayout = view.FindViewById<RelativeLayout>( Resource.Id.prayerRequest_background );
                    ControlStyling.StyleBGLayer( backgroundLayout );
                    //

                    // setup the switch background
                    backgroundLayout = view.FindViewById<RelativeLayout>( Resource.Id.switch_background );
                    ControlStyling.StyleBGLayer( backgroundLayout );

                    // setup the category background
                    backgroundLayout = view.FindViewById<RelativeLayout>( Resource.Id.spinner_background );
                    ControlStyling.StyleBGLayer( backgroundLayout );

                    // setup the text views
                    EditText firstNameText = (EditText)view.FindViewById<EditText>( Resource.Id.prayer_create_firstNameText );
                    ControlStyling.StyleTextField( firstNameText, PrayerStrings.CreatePrayer_FirstNamePlaceholderText, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );

                    EditText lastNameText = (EditText)view.FindViewById<EditText>( Resource.Id.prayer_create_lastNameText );
                    ControlStyling.StyleTextField( lastNameText, PrayerStrings.CreatePrayer_LastNamePlaceholderText, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );

                    EditText requestText = (EditText)view.FindViewById<EditText>( Resource.Id.prayer_create_requestText );
                    ControlStyling.StyleTextField( requestText, PrayerStrings.CreatePrayer_PrayerRequest, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );


                    Switch anonymousSwitch = (Switch)view.FindViewById<Switch>( Resource.Id.postAnonymousSwitch );
                    anonymousSwitch.Checked = false;
                    anonymousSwitch.CheckedChange += (object sender, CompoundButton.CheckedChangeEventArgs e ) =>
                    {
                            if( anonymousSwitch.Checked == false )
                            {
                                firstNameText.Enabled = true;
                                lastNameText.Enabled = true;

                                firstNameText.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                                lastNameText.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                            }
                            else
                            {
                                firstNameText.Enabled = false;
                                lastNameText.Enabled = false;

                                firstNameText.SetTextColor( Android.Graphics.Color.DimGray );
                                lastNameText.SetTextColor( Android.Graphics.Color.DimGray );
                            }
                    };

                    Switch publicSwitch = (Switch)view.FindViewById<Switch>( Resource.Id.makePublicSwitch );
                    publicSwitch.Checked = true;

                    TextView postAnonymousLabel = view.FindViewById<TextView>( Resource.Id.postAnonymous );
                    ControlStyling.StyleUILabel( postAnonymousLabel, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );

                    TextView publicLabel = view.FindViewById<TextView>( Resource.Id.makePublic );
                    ControlStyling.StyleUILabel( publicLabel, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );

                    // setup our category spinner
                    Spinner spinner = (Spinner)view.FindViewById<Spinner>( Resource.Id.categorySpinner );
                    ArrayAdapter adapter = new SpinnerArrayAdapter( Rock.Mobile.PlatformSpecific.Android.Core.Context, Android.Resource.Layout.SimpleListItem1 );
                    adapter.SetDropDownViewResource( Android.Resource.Layout.SimpleSpinnerDropDownItem );
                    spinner.Adapter = adapter;

                    // populate the category
                    foreach ( string category in CCVApp.Shared.Network.RockGeneralData.Instance.Data.PrayerCategories )
                    {
                        adapter.Add( category );
                    }

                    Button submitButton = (Button)view.FindViewById<Button>( Resource.Id.prayer_create_submitButton );
                    ControlStyling.StyleButton( submitButton, PrayerStrings.CreatePrayer_SubmitButtonText, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
                    submitButton.Click += (object sender, EventArgs e ) =>
                    {
                            if( (string.IsNullOrEmpty( firstNameText.Text ) == false || anonymousSwitch.Checked == true) &&
                                string.IsNullOrEmpty( requestText.Text ) == false )
                            {
                                Rock.Client.PrayerRequest prayerRequest = new Rock.Client.PrayerRequest();

                                firstNameText.Enabled = false;
                                lastNameText.Enabled = false;
                                requestText.Enabled = false;

                                // respect their privacy settings
                                if( anonymousSwitch.Checked == true )
                                {
                                    prayerRequest.FirstName = "Anonymous";
                                    prayerRequest.LastName = "Anonymous";
                                }
                                else
                                {
                                    prayerRequest.FirstName = firstNameText.Text;
                                    prayerRequest.LastName = lastNameText.Text;
                                }

                                prayerRequest.Text = requestText.Text;
                                prayerRequest.EnteredDateTime = DateTime.Now;
                                prayerRequest.ExpirationDate = DateTime.Now.AddYears( 1 );
                                prayerRequest.CategoryId = 110; //todo: Let the end user set this.
                                prayerRequest.IsActive = true;
                                prayerRequest.IsPublic = publicSwitch.Checked;
                                prayerRequest.IsApproved = false;


                                ParentTask.OnClick( this, 0, prayerRequest );
                            }
                    };

                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.RevealForTime( 3.0f );
                }
            }
        }
    }
}
